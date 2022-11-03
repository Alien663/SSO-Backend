# This WebAPI Example
## SQL Connection String

location : appsettings.json > ConnectionStrings

## Model Folder
儲存所有Model用

## call API

### GET
./API/Example/example/1/2

### POST
./API/Example

body:
```javascript
{
    TID:1,
    Name: "test",
    Des: "test"
}
```

## other setting
在startup.cs中另外有其他設定
1. 前端(靜態): ./ClienApp
2. 專案範例為前後端分離式開發，但是部屬環境是結合在一起的，所以跨網域部分才會是寫locahost:3000
3. 預設開啟跨網域存取為localhost:3000，適用於本機前後端測試環境使用，如需開啟測試環境，請修改C#或是前端的port
4. SQL連線依舊仰賴MS的SQLClient，只是資料存取使用dapper方便編寫程式碼
5. 資料進出盡量依照model設計，使資料結構嚴謹，維護時較容易理解程式碼邏輯，debug時可以依照以下寫法
6. AuthorizationFilter增加dev mode，修改true/false可以直接不判斷登入

```C#
    var data = db.Connection.Query(strSql, new { @id1, id2 }).ToList();
```"# SANW-Backend" 
"# Member-BackEnd" 
