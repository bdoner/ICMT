.POSIX:
.SUFFIXES:
.PHONY: run clean

CC		= cc
CFLAGS	= -W -g
LDLIBS	= -lm
BIND_ADDR	= 192.168.70.232

icmt-server: server.o ICMT.o
	$(CC) $(LDFLAGS) -o icmt-server server.o ICMT.o $(LDLIBS)

server.o: server.c
	$(CC) -c $(CFLAGS) server.c

ICMT.o: ICMT.c
	$(CC) -c $(CFLAGS) ICMT.c

run: icmt-server
	sudo ./icmt-server -v 4 -b $(BIND_ADDR)

clean:
	rm server icmt-server *.o
