#### build executable for windows under Windows
- go build
#### build executable for linux under Windows
- cmd /C "set "CGO_ENABLED=0" && set "GOOS=linux" && set "GOARCH=amd64" && set "GO111MODULE=on" && go build"

