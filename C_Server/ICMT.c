#include "ICMT.h"
#include <string.h>
#include <arpa/inet.h>


void fill_message_head(message_head *msg, char *icmp_msg, ssize_t size)
{
    memcpy(msg, icmp_msg + 28, sizeof(message_head));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
}

void fill_message_setup(message_setup *msg, char *icmp_msg, ssize_t size)
{
    memcpy(msg, icmp_msg + 28, sizeof(message_setup));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
}

void fill_message_data(message_data *msg, char *icmp_msg, ssize_t size)
{
    memcpy(msg, icmp_msg + 28, sizeof(message_data));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
    msg->dataLength = ntohs(msg->dataLength);
}

void fill_message_complete(message_complete *msg, char *icmp_msg, ssize_t size)
{
    memcpy(msg, icmp_msg + 28, sizeof(message_complete));
    msg->magic = ntohl(msg->magic);
    msg->sequenceNum = ntohl(msg->sequenceNum);
    msg->checksum = ntohl(msg->checksum);
}