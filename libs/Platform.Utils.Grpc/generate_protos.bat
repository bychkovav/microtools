@rem Generate the C# code for .proto files

setlocal

@rem enter this directory
cd /d %~dp0

set TOOLS_PATH=..\packages\Grpc.Tools.1.0.0\tools\windows_x64

%TOOLS_PATH%\protoc.exe -I Protos --csharp_out Generated Protos\RemoteService.proto --grpc_out Generated --plugin=protoc-gen-grpc=%TOOLS_PATH%\grpc_csharp_plugin.exe

endlocal