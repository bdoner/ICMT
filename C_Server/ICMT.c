#include "ICMT.h"
#include <sys/types.h>
#include <sys/unistd.h>
#include <string.h>


void fill_message_head(message_head_t *msg, char *icmp_msg, ssize_t size) 
{
    memcpy(msg, icmp_msg + 28, sizeof(message_head_t));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
}

void fill_message_setup(message_setup_t *msg, char *icmp_msg, ssize_t size) 
{
    memcpy(msg, icmp_msg + 28, sizeof(message_setup_t));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
}

void fill_message_data(message_data_t *msg, char *icmp_msg, ssize_t size) 
{
    memcpy(msg, icmp_msg + 28, sizeof(message_data_t));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
    msg->dataLength = ntohs(msg->dataLength);
}

void fill_message_complete(message_complete_t *msg, char *icmp_msg, ssize_t size) 
{
    memcpy(msg, icmp_msg + 28, sizeof(message_complete_t));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
    //msg->checksum = ntohl(msg->checksum);
}