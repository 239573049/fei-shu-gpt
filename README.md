# 飞书AI接入教程

首先，准备俩个账号：ChatGPT账号、飞书账号。

飞书账号请自行注册，访问链接 www.feishu.cn/ 即可登录。

第一步，飞书进入[开发者平台](https://open.feishu.cn/)。点击创建应用。

![image-20231227155605146](./img/1.jpg)

填写应用名称和描述，还有头像也可以自己修改。

![](D:\token\FeishuGpt\img\2.jpg)

然后点击左边的添加应用，添加机器人。

![](D:\token\FeishuGpt\img\3.jpg)

添加机器人权限：

im:message

im:message.group_at_msg

im:message.group_at_msg:readonly

im:message.p2p_msg

im:message.p2p_msg:readonly

im:message:send_as_bot

![](D:\token\FeishuGpt\img\4.jpg)

然后一件开通权限

![](D:\token\FeishuGpt\img\5.jpg)

然后在你的服务器中启动`FeiShuGpt`项目

```
docker run -d -p 1001:8080 --name=feishu-gpt -e APPID=xxx -e BOTNAME=xx -e APPSECRET=xxx -e GPTKEY=xxx -v ./FeiShuGpt.db:/app/FeiShuGpt.db registry.cn-shenzhen.aliyuncs.com/tokengo/feishu-gpt:latest
```

- 环境变量参数：
  - `APPID` 飞书应用Id
  - `APPSECRET` 飞书应用密钥
  - `GPTKEY` `ChatGptApi`密钥
  - `MODEL`  使用的AI模型为空则使用默认`gpt-3.5-turbo-1106`模型
  - `ENDPOINT` `OpenAI`端点 为空则使用默认
  - `MAXHISTORY` 最大聊天记录数量默认 3
  - `BOTNAME` 机器人名称（在群里使用需要，因为需要判断@的是否为机器人）

然后打开事件于回调

![](./img/6.jpg)

修改回调地址：`http://服务器ip地址:1001/api/v1/Chats`，请注意 `Encrypt Key`不要填。

![](./img/7.jpg)

点击添加事件：

![](./img/8.jpg)

搜索`接收消息` 然后添加 `接收消息 v2.0`

![](./img/9.jpg)

然后打开版本管理与发布，创建版本，输入版本号。

![](./img/10.jpg)

![](./img/11.jpg)

然后保存审核。

![](./img/12.jpg)

然后找到我们的应用

![](./img/13.jpg)

## 结尾

***源码在星球***

加入Token的星球提供技术支持12小时在线回复在线解决问题包括远程协助解决问题。

![](./xq.jpg)

技术交流群：737776595
