#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>

#define PROTO_ICMP 1
#define TRUE 1
#define FALSE 0

typedef struct {
    char magic[4];
    unsigned int sequenceNum;
    char messageType;
    char sessionId[4];
} message_head;

typedef struct {
    char magic[4];
    unsigned long sequenceNum;
    char messageType;
    char sessionId[4];
    char fileNameLength;
    char *fileName;
} message_setup;

typedef struct {
    char magic[4];
    unsigned long sequenceNum;
    char messageType;
    char sessionId[4];
    unsigned short dataLength;
    char *data;
} message_data;

typedef struct {
    char magic[4];
    unsigned long sequenceNum;
    char messageType;
    char sessionId[4];
    unsigned long checksum;
} message_completion;



int main()
{

    struct sockaddr_in sock_addr;
    sock_addr.sin_family = AF_INET;
    sock_addr.sin_port = htons(0);
    inet_aton("127.0.0.1", &sock_addr.sin_addr.s_addr);

    int sockfd = socket(AF_INET, SOCK_RAW, PROTO_ICMP);
    if (sockfd == -1)
    {
        perror("could not create socket");
        return -1;
    }
    bind(sockfd, &sock_addr, sizeof(sock_addr));

    const int BUFF_SIZE = 2000;
    char buff[BUFF_SIZE];

    while (TRUE)
    {
        //clear buffer
        memset(buff, 0, BUFF_SIZE);
        message_head msg_head;

        ssize_t recv_size = recv(sockfd, buff, BUFF_SIZE, 0);
        
        memcpy(&msg_head, buff + 28, sizeof(message_head));

        printf("message magic (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            msg_head.magic[0], msg_head.magic[1], msg_head.magic[2], msg_head.magic[3]);
        printf("\tseqNum (ulong): %lu\n", msg_head.sequenceNum);
        printf("\ttype (char): %d\n", msg_head.messageType);
        printf("\tsessionId (byte[4]): [%02hhx, %02hhx, %02hhx, %02hhx]\n", 
            msg_head.sessionId[0], msg_head.sessionId[1], msg_head.sessionId[2], msg_head.sessionId[3]);

        for (int i = 0; i < recv_size; i++)
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
}