.POSIX:
.SUFFIXES:

CC		= cc
CFLAGS	= -W -g
LDLIBS	= -lm
BIND_ADDR	= 192.168.70.232

server: server.o ICMT.o
	$(CC) $(LDFLAGS) -o server server.o ICMT.o $(LDLIBS)

server.o: server.c
	$(CC) -c $(CFLAGS) server.c

ICMT.o: ICMT.c
	$(CC) -c $(CFLAGS) ICMT.c

run: server
	sudo ./server $(BIND_ADDR)

clean:
	rm server *.o
