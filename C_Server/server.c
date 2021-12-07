#include "ICMT.h"
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <string.h>

#define PROTO_ICMP 1
#define TRUE 1
#define FALSE 0

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

unsigned int checksum_file(int fd_file)
{

    return 1;
}

void print_msg(char *buff, int buffsize) {

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

int main(int argc, char **argv)
{
    for(int i = 0; i < argc; i++) {
        printf("argv[%d] = '%s'\n", i, argv[i]);
    }

    //printf("uid: %d\neuid: %d\nSUDO_UID: %s\n", getuid(), geteuid(), getenv("SUDO_UID"));

    int sockfd = bind_socket(argv[1]);
    drop_privs();
    create_rcv_dir();

    const int BUFF_SIZE = 1500;
    char buff[BUFF_SIZE];
    char icmp_msg[BUFF_SIZE];
    
    int fd = -1; // file to write to
    message_setup_t msg_setup; // contains sessionId
    long lastSeqNum = -1; // check sequence
    while (TRUE)
    {
        message_head_t msg_head;

        // clear buffer
        memset(buff, 0, BUFF_SIZE);
        memset(icmp_msg, 0, BUFF_SIZE);
        
        ssize_t recv_size = recv(sockfd, buff, BUFF_SIZE, 0);
        memcpy(&icmp_msg, buff + 28  /* skip IP header */, BUFF_SIZE - 28);

        memcpy(&msg_head, icmp_msg, sizeof(message_head_t));

        if(0 != memcmp(msg_head.magic, ICMT_MACIG, 4)) {
            continue;
        }
       

        printf("message magic (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            msg_head.magic[0], msg_head.magic[1], msg_head.magic[2], msg_head.magic[3]);
        printf("\tseqNum (uint): %u\n", msg_head.sequenceNum);
        printf("\ttype (char): %d\n", msg_head.messageType);
        printf("\tsessionId (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            msg_head.sessionId[0], msg_head.sessionId[1], msg_head.sessionId[2], msg_head.sessionId[3]);
        
        //print_msg(buff, recv_size);

        if(msg_head.sequenceNum <= lastSeqNum) {
            printf("last seq was %d this seq is %u\n", lastSeqNum, msg_head.sequenceNum);
            continue;
        }

        lastSeqNum = msg_head.sequenceNum;

        //printf("PAST seq-check:\n");

        if (msg_head.messageType == MSGTYPE_SETUP) 
        {
            printf("SETUP:\n");
            // fill struct
            memcpy(&msg_setup, icmp_msg, sizeof(message_setup_t));
            // set filename terminating null byte
            msg_setup.fileName[msg_setup.fileNameLength + 1] = 0;

            // get fd for final file
            fd = create_file(msg_setup.fileName);
        } 
        else if(msg_head.messageType == MSGTYPE_DATA)
        {
            printf("DATA:\n");

            message_data_t msg_data;
            memcpy(&msg_data, icmp_msg, sizeof(message_data_t));

            printf("sizeof(icmp_msg): %d\n", sizeof(icmp_msg));
            printf("sizeof(message_data_t): %d\n", sizeof(message_data_t));
            printf("sizeof(unsigned short): %d\n", sizeof(unsigned short));
            printf("fd: %d\n", fd);
            printf("msg_data.dataLength: %hu\n", msg_data.dataLength);
            print_msg(&msg_data, sizeof(message_data_t));

            ssize_t w = write(fd, msg_data.data, 131);
            if(w == -1) 
            {
                perror("error writing file");
            }

            printf("Wrote %d bytes of %d\n", w, msg_data.dataLength);

        }
        else if(msg_head.messageType == MSGTYPE_COMPLETE)
        {
            printf("COMPLETE:\n");

            message_complete_t msg_complete;
            memcpy(&msg_complete, icmp_msg, sizeof(message_complete_t));

            fflush(fd);

            // checksum file before closing fd
            unsigned int checksum = checksum_file(fd);
            printf("Server checksum:\t%04x\nClient checksum:\t%04x\n", checksum, msg_complete.checksum);

            close(fd);
        }
        
    }
}