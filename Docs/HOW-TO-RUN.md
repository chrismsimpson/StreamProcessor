# How to run

If you've set up `.zprofile`:

```
sp --read-file ./Samples/transactions.json
sp --nft 0xA000000000000000000000000000000000000000
sp --nft 0xB000000000000000000000000000000000000000
sp --nft 0xC000000000000000000000000000000000000000
sp --nft 0xD000000000000000000000000000000000000000
sp --read-inline '{ "Type": "Mint", "TokenId": "0xD000000000000000000000000000000000000000", "Address": "0x1000000000000000000000000000000000000000" }'
sp --nft 0xD000000000000000000000000000000000000000
sp --wallet 0x3000000000000000000000000000000000000000
sp --reset
sp --wallet 0x3000000000000000000000000000000000000000
```

Otherwise:

```
dotnet run --project ./Sources/StreamProcessor.csproj --reset
dotnet run --project ./Sources/StreamProcessor.csproj --read-file ./Samples/transactions.json
dotnet run --project ./Sources/StreamProcessor.csproj --nft 0xA000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --nft 0xB000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --nft 0xC000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --nft 0xD000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --read-inline '{ "Type": "Mint", "TokenId": "0xD000000000000000000000000000000000000000", "Address": "0x1000000000000000000000000000000000000000" }'
dotnet run --project ./Sources/StreamProcessor.csproj --nft 0xD000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --wallet 0x3000000000000000000000000000000000000000
dotnet run --project ./Sources/StreamProcessor.csproj --reset
dotnet run --project ./Sources/StreamProcessor.csproj --wallet 0x3000000000000000000000000000000000000000
```

Have also set up 10 steps to run though for VSCode (in `launch.json`).