# 该脚本应置于

# 服务器IP地址
ServerNetworkAddress=150.158.44.119
# 房间管理服务端进程的端口号
MatchServerPort=8000
# 游戏服务端可执行文件的绝对路径
GameServerPath=/home/ubuntu/game/gameServer/UrbanOutlastServer.x86_64
# 游戏服务端进程可使用的端口号范围，格式为"起始端口-结束端口"
GameServerPortRange=8001-8050


chmod +x ./gameMatchServer/MatchManager.x86_64
chmod +x ./gameServer/UrbanOutlastServer.x86_64
./gameMatchServer/MatchManager.x86_64 $ServerNetworkAddress $MatchServerPort $GameServerPath $GameServerPortRange