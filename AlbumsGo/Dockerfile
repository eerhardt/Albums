# Build the InstanceAvailability server container, completely inside the container
#
#


######################
# STEP 1 build binary
######################
FROM golang:alpine AS builder
LABEL stage=builder

RUN apk add git

# copy needed project files:
ADD . /go/src/albums/

# git should also use our gitconfig
RUN git config --global include.path /mygitconfig

WORKDIR /go/src/albums
RUN go install -v ./...

RUN GOOS=linux GOARCH=amd64 go build -ldflags="-w -s" -o /go/bin/albums

##########################
# STEP 2 build small image
##########################
FROM alpine
# copy executable.
COPY --from=builder /go/bin/albums /bin/albums
# set entry point
ENTRYPOINT ["/bin/albums"]

