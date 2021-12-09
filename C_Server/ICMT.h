#ifndef ICMT_H
#define ICMT_H

typedef char msg_type_t;
const msg_type_t MSGTYPE_SETUP = 1;
const msg_type_t MSGTYPE_DATA = 2;
const msg_type_t MSGTYPE_COMPLETE = 3;

const char ICMT_MACIG_BYTES[4] = {0x69, 0x63, 0x6d, 0x74};
const unsigned int ICMT_MAGIC = 1768123764;

typedef struct  __attribute__((packed)) {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
} message_head_t;

typedef struct  __attribute__((packed)) {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    char fileNameLength;
    char fileName[256];
} message_setup_t;

typedef struct __attribute__((packed)) {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    unsigned short dataLength;
    char data[1500]; //probably never more than 1500 bytes due to the MTU limit
} message_data_t;

typedef struct  __attribute__((packed)) {
    unsigned int magic;
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    unsigned int checksum;
} message_complete_t;

#endif