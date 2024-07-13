# KirbyLib
 A .NET library for working with various file formats found in Kirby games.

## Supported File Formats
All sections of all formats are fully supported. The library is intended to match internal names as close as possible for ease of modding and research.

|                             | 2D Map | 3D Map | Cinemo | FDG | Generic Archives | Msg Filter | Yaml | XData |
| :-------------------------- | :----: | :----: | :----: | :-: | :--------------: | :--------: | :--: | :---: |
| Return to Dream Land        |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Dream Collection            |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Triple Deluxe               |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Drum Dash                   |   ❌   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Fighters                    |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Drum Dash Deluxe            |   ❌   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Fighters Deluxe             |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ➖  |  ✔️   |
| Planet Robobot              |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ✔️  |  ✔️   |
| Team Clash                  |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ✔️  |  ✔️   |
| Team Clash Deluxe           |   ✔️   |  ➖   |   ➖   | ✔️ |         ✔️       |     ➖     |  ✔️  |  ✔️   |
| Star Allies                 |   ✔️   |  ✔️   |   ✔️[^1]   | ✔️ |         ➖       |     ✔️     |  ✔️  |  ✔️   |
| Super Clash                 |   ✔️   |  ➖   |   ✔️[^1]   | ✔️ |         ➖       |     ✔️     |  ✔️  |  ✔️   |
| Fighters 2                  |   ✔️   |  ➖   |   ✔️   | ✔️ |         ✔️       |     ✔️     |  ✔️  |  ✔️   |
| Forgotten Land              |   ➖   |  ➖   |   ✔️   | ✔️ |         ➖       |     ✔️     |  ✔️  |  ✔️   |
| Dream Buffet                |   ➖   |  ➖   |   ✔️   | ✔️ |         ➖       |     ✔️     |  ✔️  |  ✔️   |
| Return to Dream Land Deluxe |   ✔️   |  ➖   |   ✔️   | ✔️ |         ✔️       |     ✔️     |  ✔️  |  ✔️   |

### For HAL's scripting bytecode, Mint/Basil, please see [MintWorkshop](https://github.com/firubii/MintWorkshop).

[^1]: Cinemo support is in a separate class, [`CinemoKSA`](https://github.com/firubii/KirbyLib/blob/main/KirbyLib/CinemoKSA.cs), due to large format differences.
