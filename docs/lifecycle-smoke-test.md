# 生命周期清理手动 Smoke Test

适用范围：P2 启动幂等与生命周期清理。目标是确认重载 mod、断线重连、传输模式切换后，不继承上一轮网络会话状态。

## 准备

- 使用同一构建包，主机和客户端版本一致。
- 打开运行时日志，重点 grep：`[NET_STATE]`、`[STEAM_JOIN]`、`[PLAYER_SYNC]`、`[AI_SYNC]`。
- 每一步开始前记录当前传输模式：Direct 或 SteamP2P。

## 重载与主菜单往返

1. 启动游戏并进入主菜单。
2. 确认日志只出现一次 `COOPManager initialized`，重复启用时最多出现 `InitManager reentry`。
3. 进入地图后返回主菜单，再进入地图。
4. 检查场景投票、scene gate、AI snapshot 请求没有沿用上一张图的状态。
5. 使用运行时日志或对象枚举确认没有生成多个 `COOP_MOD_1` / `COOP_MOD_`。

## LAN 断线重连

1. 主机 Direct 模式创建服务。
2. 客户端手动 IP 连接，确认双方可见玩家。
3. 客户端断开，确认日志出现 `network stopped` 或 `peer disconnected`，并有 `reset coop session state`。
4. 客户端重新连接同一主机。
5. 确认远端玩家代理、AI replica、loot viewer、scene ready 都是新会话状态。

## SteamP2P 模式切换

1. Direct 模式启动后停止网络。
2. 切换 SteamP2P，确认日志出现 `transport mode changed`。
3. 创建 lobby，再停止网络。
4. 确认 pending join 被取消、旧 lobby 被离开、endpoint mapper 和 P2P session state 被清理。
5. 再切回 Direct 并创建服务，确认不会尝试复用旧 Steam endpoint。

## 迟加入与换图

1. 主机进入 raid 并等待 AI 正常生成。
2. 客户端迟加入，确认收到 AI snapshot 并看到同图队友。
3. 主机发起换图投票，客户端确认后进入新图。
4. 确认旧图 remote proxy / AI replica 被清理，同图后重新创建。

## 失败路径

1. 占用 Direct 端口后尝试创建主机，确认 UI 和日志提示启动失败。
2. Steam 未初始化或 P2P 不可用时尝试 SteamP2P，确认不会进入假联网状态。
3. 密码错误或加入超时后再次点击加入，确认 pending join 没有卡死。
