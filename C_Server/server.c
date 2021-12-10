#include "ICMT.h"
#include "server.h"

#include <sys/stat.h>
#include <sys/unistd.h>
#include <sys/types.h>
// #include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <string.h>

#define DEBUG 0

#define BUFF_SIZE 1500

int main(int argc, char **argv)
{
    #if DEBUG
    for(int i = 0; i < argc; i++) {
        printf("argv[%d] = '%s'\n", i, argv[i]);
    }
    #endif

    //printf("uid: %d\neuid: %d\nSUDO_UID: %s\n", getuid(), geteuid(), getenv("SUDO_UID"));

    int sockfd = bind_socket(argv[1]);
    drop_privs();
    create_rcv_dir();

    char buff[BUFF_SIZE];
    
    int fd = -1; // file to write to
    message_setup_t msg_setup; // contains sessionId
    long lastSeqNum = -1; // check sequence
    while (1)
    {
        message_head_t msg_head;

        // clear buffer
        memset(buff, 0, BUFF_SIZE);
        
        ssize_t recv_size = recv(sockfd, buff, BUFF_SIZE, 0);
        fill_message_head(&msg_head, buff, recv_size);

        print_msg(buff + 28, recv_size - 28);

        if(msg_head.magic != ICMT_MAGIC) {
            continue;
        }       

       char *mg_hdr = (char*)&msg_head.magic;

        #if DEBUG
        printf("message magic (unsigned int): %u\n",  msg_head.magic);
        printf("message magic (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            mg_hdr[0], mg_hdr[1], mg_hdr[2], mg_hdr[3]);
        printf("seqNum (uint): %u\n", msg_head.sequenceNum);
        printf("type (char): %d\n", msg_head.messageType);
        printf("sessionId (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            msg_head.sessionId[0], msg_head.sessionId[1], msg_head.sessionId[2], msg_head.sessionId[3]);
        #endif

        if(msg_head.sequenceNum <= lastSeqNum) {
            #if DEBUG
            printf("last seq was %ld this seq is %u\n", lastSeqNum, msg_head.sequenceNum);
            #endif
            continue;
        }

        lastSeqNum = msg_head.sequenceNum;

        //printf("PAST seq-check:\n");

        if (msg_head.messageType == MSGTYPE_SETUP) 
        {
            #if DEBUG
            printf("SETUP:\n");
            #endif

            // fill struct
            fill_message_setup(&msg_setup, buff, sizeof(message_setup_t));

            // set filename terminating null byte
            msg_setup.fileName[msg_setup.fileNameLength + 1] = 0;

            // get fd for final file
            fd = create_file(msg_setup.fileName);
        } 
        else if(msg_head.messageType == MSGTYPE_DATA)
        {
            #if DEBUG
            printf("DATA:\n");
            #endif

            message_data_t msg_data;
            fill_message_data(&msg_data, buff, sizeof(message_data_t));

            #if DEBUG
            printf("sizeof(message_data_t): %ld\n", sizeof(message_data_t));
            printf("fd: %d\n", fd);
            printf("msg_data.dataLength: %hu\n", msg_data.dataLength);

            #endif

            ssize_t w = write(fd, msg_data.data, msg_data.dataLength);
            if(w == -1) 
            {
                perror("error writing file");
            }

            #if DEBUG
            printf("Wrote %ld bytes of %d\n", w, msg_data.dataLength);
            #endif
        }
        else if(msg_head.messageType == MSGTYPE_COMPLETE)
        {
            #if DEBUG
            printf("COMPLETE:\n");
            #endif

            message_complete_t msg_complete;
            fill_message_complete(&msg_complete, buff, sizeof(message_complete_t));

            fsync(fd);

            // checksum file before closing fd
            unsigned int checksum = checksum_file(fd);
            printf("Server checksum:\t%04x\nClient checksum:\t%04x\n", checksum, msg_complete.checksum);

            close(fd);

            lastSeqNum = -1;
        }
        
    }
}

// End of main ---------------------------------------------

int bind_socket(char *addr) 
{
    struct sockaddr_in sock_addr;
    sock_addr.sin_family = AF_INET;
    sock_addr.sin_port = htons(0);
    inet_aton(addr, &sock_addr.sin_addr.s_addr);


    int sockfd = socket(AF_INET, SOCK_RAW, PROTO_ICMP);
    if (sockfd == -1)
    {
        perror("could not create socket");
        exit(-1);
    }
    
    int bindres = bind(sockfd, &sock_addr, sizeof(sock_addr));
    if(bindres == -1) 
    {
        perror("could not bind socket");
        exit(-1);
    }

    return sockfd;
}


void create_rcv_dir() 
{
    const char rf[] = "./received_files/";

    struct stat st = {0};
    if(-1 != stat(rf, &st)) 
    {
        return;
    }

    int fd = mkdir(rf, 0755);
    if(fd < 0) {
        perror("could not create './received_files' directory");
        exit(-1);
    }
}


int create_file(char *fileName)
{
    const char rf[] = "./received_files/";
    char *finalPath = malloc(strlen(rf) + strlen(fileName) + 1);
    strcat(finalPath, rf);
    strcat(finalPath, fileName);
    finalPath[strlen(rf) + strlen(fileName)] = 0;

    unlink(finalPath);
    printf("creating file %s\n", finalPath);

    int fd = open(finalPath, O_RDWR | O_CREAT, 0644);
    free(finalPath);

    if(fd < 0) {
        perror("could not create file");
        exit(-1);
    }

    return fd;
}

void drop_privs()
{

    char *sudo_gid = getenv("SUDO_GID");
    int gid = getgid();

    if(sudo_gid != NULL && 0 < strlen(sudo_gid)) {
        gid = atoi(sudo_gid);
    }

    if(setgid(gid) < 0)
    {
        perror("could not drop group privs");
    }


    char *sudo_uid = getenv("SUDO_UID"); 
    int uid = getuid();

    if(sudo_uid != NULL && 0 < strlen(sudo_uid)) {
        uid = atoi(sudo_uid);
    }

    if(setuid(uid) < 0)
    {
        perror("could not drop user privs");
    }
}

unsigned int checksum_file(int fd)
{
    printf("generating checksum for file..\n");
    lseek(fd, 0, SEEK_SET);

    unsigned int checksum = 0;
    unsigned int c;
    ssize_t r = 0;
    unsigned int its = 0, loop = 0;

    char buff[4096];
    while(0 < (r = read(fd, buff, 4096)))
    {
        if (r < 4096) 
        {
            memset(buff + r, 0, 4096 - r);
        }

        for(ssize_t i = 0; i < r; i += 4)
        {
            c = *(unsigned int *)(buff + i);
            checksum ^= c;

            if (its++ % 1000000 == 0) 
            {
                printf("loop: %u\niterations: %u\nr: %ld\nc: %u\nchecksum: %u\n\n",
                    loop, its, r, c, checksum);
            }

            c = 0;
        }
        loop++;
    }

    if(r == -1) 
    {
        perror("error reading file for checksumming");
    }

    return checksum;
}

void print_msg(char *buff, int buffsize) 
{
    #if ! DEBUG
    return;
    #endif

    for (int i = 0; i < buffsize; i++)
        {
            if (i != 0 && i % 8 == 0)
            {
                if (i % 32 == 0)
                {
                    printf("\n");
                }
                else if (i % 16 == 0)
                {
                    printf("\t");
                }
                else
                {
                    printf(" ");
                }
            }
            printf("%02hhx ", buff[i]);
        }

        printf("\n-----\n");
}