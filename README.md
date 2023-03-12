# Aliyun-DDNS

阿里云 DDNS 工具, 用于动态域名解析.

## 特性

- 更新设备的 IPv6 地址解析
- 支持Windows/Linux/MacOS, 无需Docker

## 用法

1. 下载 [Release](https://github.com/jqknono/aliyun-ddns/releases)
2. 配置参数运行, `ddns <list | update> -id <AccessKey ID> -key <AccessKey> -domain <domain> [-subdomain <subdomain>] [-record_id <record_id>]`
   - e.g. `ddns update -id LTAI5tLa55eddic4BKU2LmUi -key w4IcEzgEVbb8ErXd9ghxtZbJfkHIoi -domain jqknono.com -subdomain zhangsanshome`

## 安全性说明

- **Aliyun-DDNS**代码简洁, 不收集任何信息

建议用户使用 RAM 访问控制创建专用于 DDNS 的用户, 用户权限仅授权 DNS 相关权限, **Aliyun-DDNS**最多需要使用阿里云的以下**3**个 API.

- `DescribeDomainRecords`
- `UpdateDomainRecord`
- `AddDomainRecord`

设置地址: https://ram.console.aliyun.com/users

**为了安全起见, 请不要使用主账号的 AccessKey.**

如果仅有更新域名解析的需求, 可以仅授权`UpdateDomainRecord`权限, 但需要先自行手动增加解析记录.

## 其他说明

- 解析更改不会立刻生效, 会在十分钟内生效.
- 所使用网络是否支持 IPv6, 可访问[https://ipw.cn/], 自行检查.

## 付费服务支持

一级域名的购买繁琐, 并且需要备案, 如您没有自己的一级域名, 但需要一个固定可记忆的解析地址, 可以向我付费申请域名, 价格为 **100 元/永久**. 可获得一个二级域名的**IPv6**解析记录, 如 `zhangsanshome.jqknono.com`

提供服务流程:

- 邮件联系: [jqknono@gmail.com](mailto:jqknono@gmail.com), 说明需要的域名.
- 获取一个二级域名, 如 `zhangsanshome.jqknono.com`
- 获得该二级域名解析的记录id: `817133929644410109110`
  - 使用**Aliyun-DDNS**更新解析记录, `ddns update -id LTAI5tLa55eddic4BKU2LmUi -key w4IcEzgEVbb8ErXd9ghxtZbJfkHIoi -domain jqknono.com -record_id 817133929644410109110`
- 获得设置开机自启设置命令, 定时更新任务设置命令
- 解析成功后付费
