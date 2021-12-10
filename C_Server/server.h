#ifndef SERVER_H
#define SERVER_H

#define PROTO_ICMP 1

int bind_socket(char *addr);
void create_rcv_dir();
int create_file(char *fileName);
void drop_privs();
unsigned int checksum_file(int fd_file);
void print_msg(char *buff, int buffsize);

#endif