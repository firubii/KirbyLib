# KirbyLib
 A .NET library for working with various file formats found in Kirby games.

## Supported File Formats and Versions
All sections of all formats are fully supported.

* **2D Map Files** for the following games:
	* [Kirby's Return to Dream Land](KirbyLib/Mapping/MapRtDL.cs)
	* [Kirby's Dream Collection](KirbyLib/Mapping/MapRtDL.cs)
	* [Kirby: Triple Deluxe](KirbyLib/Mapping/MapTDX.cs)
	* [Kirby Fighters](KirbyLib/Mapping/MapFighters.cs)
	* [Kirby Fighters Deluxe](KirbyLib/Mapping/MapFighters.cs)
	* [Kirby: Planet Robobot](KirbyLib/Mapping/MapKPR.cs)
	* [Team Kirby Clash](KirbyLib/Mapping/MapClash.cs)
	* [Team Kirby Clash Deluxe](KirbyLib/Mapping/MapClashDeluxe.cs)
	* [Kirby Star Allies](KirbyLib/Mapping/MapKSA.cs)
	* [Super Kirby Clash](KirbyLib/Mapping/MapClashSuper.cs)
	* [Kirby Fighters 2](KirbyLib/Mapping/MapFighters.cs)
	* [Kirby's Return to Dream Land Deluxe](KirbyLib/Mapping/MapRtDL.cs)
* [**3D Map Files**](KirbyLib/Mapping/Map3D.cs) for **Kirby Star Allies** world maps
* **Cinemo Dynamics** (CND)
	* The [early Kirby Star Allies and Super Kirby Clash iteration](KirbyLib/CinemoKSA.cs) is handled separately from [the one introduced in Kirby Fighters 2](KirbyLib/Cinemo.cs) for cleanliness and ease of use.
* [**FDG Preload Files**](KirbyLib/FDG.cs)
	* Version 2 (Kirby's Return to Dream Land to Kirby Star Allies)
	* Version 3 (Kirby and the Forgotten Land and later)
* [**Generic Archives**](KirbyLib/GenericArchive.cs)
	* Used as a generic container across HAL Laboratory games, usually for parameter files (i.e. Kirby's Return to Dream Land)
* [**Msg Filter**](KirbyLib/MsgFilter.cs)
	* Used to control which glyphs fonts will load.
* [**Yaml**](KirbyLib/Yaml.cs)
	* Version 2 (Kirby: Planet Robobot to Kirby Fighters 2)
	* Version 4 (Kirby and the Forgotten Land and Kirby's Dream Buffet)
	* Version 5 (Kirby's Return to Dream Land Deluxe)
* [**XData Header**](KirbyLib/XData.cs)
	* Version 2.0 (Kirby's Return to Dream Land to Team Kirby Clash Deluxe)
	* Version 4.0 (Kirby Battle Royale to Kirby Fighters 2)
	* Version 5.0 (Kirby and the Forgotten Land and later)
