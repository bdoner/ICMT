#ifndef ICMT_H
#define ICMT_H

#include <sys/types.h>


typedef char msg_type;
static const msg_type MSGTYPE_SETUP = 1;
static const msg_type MSGTYPE_DATA = 2;
static const msg_type MSGTYPE_COMPLETE = 3;

static const char ICMT_MACIG_BYTES[4] = {0x69, 0x63, 0x6d, 0x74};
static const unsigned int ICMT_MAGIC = 1768123764;

typedef struct  __attribute__((packed)) message_head {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type messageType;
    char sessionId[4];
} message_head;

typedef struct __attribute__((packed)) message_setup {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type messageType;
    char sessionId[4];
    char fileNameLength;
    char fileName[256];
} message_setup;

typedef struct __attribute__((packed)) message_data {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type messageType;
    char sessionId[4];
    unsigned short dataLength;
    char data[1500]; //probably never more than 1500 bytes due to the MTU limit
} message_data;

typedef struct __attribute__((packed)) message_complete {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type messageType;
    char sessionId[4];
    unsigned int checksum;
} message_complete;

void fill_message_head(message_head *msg, char *icmp_msg, ssize_t size);
void fill_message_setup(message_setup *msg, char *icmp_msg, ssize_t size);
void fill_message_data(message_data *msg, char *icmp_msg, ssize_t size);
void fill_message_complete(message_complete *msg, char *icmp_msg, ssize_t size);

#endif