
typedef char msg_type_t;
const msg_type_t MSGTYPE_SETUP = 1;
const msg_type_t MSGTYPE_DATA = 2;
const msg_type_t MSGTYPE_COMPLETE = 3;

const char ICMT_MACIG[4] = {0x69, 0x63, 0x6d, 0x74};


typedef struct {
    char magic[4];
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
} message_head_t;

typedef struct {
    char magic[4];
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    char fileNameLength;
    char fileName[256];
} message_setup_t;

typedef struct {
    char magic[4];
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    unsigned short dataLength;
    char data[1500]; //probably never more than 1500 bytes due to the MTU limit
} message_data_t;

typedef struct {
    char magic[4];
    unsigned int sequenceNum;
    msg_type_t messageType;
    char sessionId[4];
    unsigned int checksum;
} message_complete_t;