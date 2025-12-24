using KirbyLib;
using KirbyLib.IO;
using KirbyLib.Mapping;
using KirbyLib.Mint;
using System;
using System.IO;

namespace KirbyLib_Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void MintRtDLTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dreamland\DATA\files\mint\Archive.bin";

            ArchiveRtDL mint;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                mint = new ArchiveRtDL(reader);

            Console.WriteLine($"Version: {mint.GetVersionString()}");
            if (mint.ModuleExists("User.Tsuruoka.MintTest"))
            {
                Console.WriteLine("User.Tsuruoka.MintTest:");
                MintObject obj = mint["User.Tsuruoka.MintTest"][0];
                for (int i = 0; i < obj.Variables.Count; i++)
                {
                    MintVariable var = obj.Variables[i];
                    Console.WriteLine($"  {var.Type} {var.Name}");
                }
                Console.WriteLine();
                for (int f = 0; f < obj.Functions.Count; f++)
                {
                    MintFunction func = obj.Functions[f];
                    Console.WriteLine("  " + func.Name + " {");
                    for (int i = 0; i < func.Data.Length; i += 4)
                    {
                        Console.WriteLine($"    {func.Data[i + 0]:X2} {func.Data[i + 1]:X2} {func.Data[i + 2]:X2} {func.Data[i + 3]:X2}");
                    }
                    Console.WriteLine("  }");
                    Console.WriteLine();
                }
            }
        }

        [TestMethod]
        public void MintRtDLIOTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dreamland\DATA\files\mint\Archive.bin";
            const string OUT_PATH = "rtdl_mint_out_test.bin";

            ArchiveRtDL archive;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive = new ArchiveRtDL(reader);

            Console.WriteLine("Successfully read archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                archive.Write(writer);

            Console.WriteLine("Successfully wrote archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive.Read(reader);

            Console.WriteLine("Successfully re-read archive");
        }

        [TestMethod]
        public void MintKSATest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\mint\Step.bin";
            const string MODULE_NAME = "Scn.Step.Hero.Common.StateSliding";
            const string FUNCTION_NAME = "procAnim";

            Archive mint;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                mint = new Archive(reader);

            Console.WriteLine($"Version: {mint.GetVersionString()}");
            if (mint.ModuleExists(MODULE_NAME))
            {
                Module mod = mint.GetModule(MODULE_NAME);
                if (mod[0].FunctionExistsByShortName(FUNCTION_NAME))
                {
                    MintFunction func = mod[0].GetFunctionByShortName(FUNCTION_NAME);
                    Console.WriteLine($"{mod.Name}.{func.NameWithoutType()}:");
                    for (int i = 0; i < func.Data.Length; i += 4)
                    {
                        Console.WriteLine($"  {func.Data[i + 0]:X2} {func.Data[i + 1]:X2} {func.Data[i + 2]:X2} {func.Data[i + 3]:X2}");
                    }
                }
            }
        }

        [TestMethod]
        public void MintKSAIOTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\mint\Step.bin";
            const string OUT_PATH = "ksa_mint_step_out.bin";

            Archive archive;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive = new Archive(reader);

            Console.WriteLine("Successfully read archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                archive.Write(writer);

            Console.WriteLine("Successfully wrote archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive.Read(reader);

            Console.WriteLine("Successfully re-read archive");
        }

        [TestMethod]
        public void MintKatFLIOTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby and the Forgotten Land\romfs\basil\Scn.bin";
            const string OUT_PATH = "katfl_basil_scn_out.bin";

            Archive archive;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive = new Archive(reader);

            Console.WriteLine("Successfully read archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                archive.Write(writer);

            Console.WriteLine("Successfully wrote archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive.Read(reader);

            Console.WriteLine("Successfully re-read archive");
        }

        [TestMethod]
        public void MintRtDLDXIOTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\basil\ScnStep.bin";
            const string OUT_PATH = "rtdldx_basil_scnstep_out.bin";

            Archive archive;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive = new Archive(reader);

            Console.WriteLine("Successfully read archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                archive.Write(writer);

            Console.WriteLine("Successfully wrote archive");

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                archive.Read(reader);

            Console.WriteLine("Successfully re-read archive");
        }

        [TestMethod]
        public void YamlIOTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\exlyml\Omen\MasterSheet.bin";
            const string OUT_PATH = "yaml_out_test.bin";

            Yaml yaml;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                yaml = new Yaml(reader);

            Console.WriteLine(yaml);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                yaml.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                yaml.Read(reader);

            Console.WriteLine(yaml);
        }

        [TestMethod]
        public void YamlTest()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\exlyml\Omen\MasterSheet.bin";

            Yaml yaml;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                yaml = new Yaml(reader);

            Console.WriteLine("XData Version: " + yaml.XData.Version[0] + "." + yaml.XData.Version[1]);
            Console.WriteLine("Yaml Version: " + yaml.Version);
            Console.WriteLine(yaml);
        }

        void RecurseYaml(YamlNode node, int indent)
        {
            switch (node.Type)
            {
                default:
                    Console.WriteLine(node.ToString());
                    break;
                case YamlType.Hash:
                case YamlType.Array:
                    Console.WriteLine(new string('\t', indent));
                    for (int i = 0; i < node.Length; i++)
                    {
                        Console.Write(new string('\t', indent) + node.Key(i) + ": ");
                        RecurseYaml(node[i], indent + 1);
                    }
                    break;
            }
        }

        [TestMethod]
        public void MapTestWii()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dreamland\DATA\files\map";

            string[] maps = Directory.GetFiles(PATH, "*.dat", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                Console.WriteLine(maps[i].Remove(0, PATH.Length));
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapRtDL map = new MapRtDL(reader);

                    PrintMapInfo(map);
                }
            }
        }

        [TestMethod]
        public void MapTestDeluxe()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\map\Step";

            string[] maps = Directory.GetFiles(PATH, "*.bin", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                Console.WriteLine(maps[i].Remove(0, PATH.Length));
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapRtDL map = new MapRtDL(reader);

                    PrintMapInfo(map);
                }
            }

            /*
            const string IN_PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\map\Step\Level1\Stage1\Step02.bin";
            const string OUT_PATH = "DX_L1S1S2.bin";

            MapRtDL map;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);

            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);
            */
        }

        [TestMethod]
        public void MapTestTDX()
        {
            const string PATH = @"D:\Game Dumps\Kirby Triple Deluxe\romfs\map\Step";

            string[] maps = Directory.GetFiles(PATH, "*.dat", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                Console.WriteLine(maps[i].Remove(0, PATH.Length));
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapTDX map = new MapTDX(reader);

                    Console.WriteLine($"\t- Width: {map.Width}");
                    Console.WriteLine($"\t- Height: {map.Height}");

                    Console.WriteLine($"\t- BGM:   {map.BGM}");
                    Console.WriteLine($"\t- Unk1:  {map.Unknown1}");
                    Console.WriteLine($"\t- ScreenSplitKind:  {map.ScreenSplitKind}");
                    Console.WriteLine($"\t- LanePairKind:  {map.LanePairKind}");
                    Console.WriteLine($"\t- CustomRespawn:  {map.CustomRespawn}");
                    Console.WriteLine($"\t- RespawnStepShift:  {map.RespawnStepShift}");
                    Console.WriteLine($"\t- RespawnStartPortal:  {map.RespawnStartPortal}");
                    Console.WriteLine($"\t- Unk7:  {map.Unknown7}");
                    Console.WriteLine($"\t- Unk8:  {map.Unknown8}");
                    Console.WriteLine($"\t- BlankSpaceGridNum:  {map.BlankSpaceGridNum}");
                    Console.WriteLine($"\t- Unk10:  {map.Unknown10}");
                    Console.WriteLine($"\t- SystemLayout:  {map.SystemLayout}");

                    /*
                    for (int j = 0; j < map.Enemies.Length; j++)
                    {
                        var enemy = map.Enemies[j];
                        Console.WriteLine($"\t- Enemy {j}");
                        Console.WriteLine($"\t\t- Kind: {enemy.Name}");
                        Console.WriteLine($"\t\t- Variation: {enemy.Variation}");
                        Console.WriteLine($"\t\t- Param 1: {enemy.Param1}");
                        Console.WriteLine($"\t\t- Param 2: {enemy.Param2}");
                        Console.WriteLine($"\t\t- Param 3: {enemy.Param3}");
                        Console.WriteLine($"\t\t- X: {enemy.X}");
                        Console.WriteLine($"\t\t- Y: {enemy.Y}");
                        Console.WriteLine($"\t\t- Unknown: {enemy.Unknown}");
                        Console.WriteLine($"\t\t- Terrain Group: {enemy.TerrainGroup}");
                    }
                    */
                    /*
                    for (int j = 0; j < map.CarryItems.Length; j++)
                    {
                        var item = map.CarryItems[j];
                        Console.WriteLine($"\t- Carry Item {j}:");
                        Console.WriteLine($"\t\t- Kind: {item.Kind}");
                        Console.WriteLine($"\t\t- Variation: {item.Variation}");
                        Console.WriteLine($"\t\t- Can Respawn: {item.CanRespawn}");
                        Console.WriteLine($"\t\t- X: {item.X}");
                        Console.WriteLine($"\t\t- Y: {item.Y}");
                    }

                    for (int j = 0; j < map.Items.Length; j++)
                    {
                        var item = map.Items[j];
                        Console.WriteLine($"\t- Item {j}:");
                        Console.WriteLine($"\t\t- Kind: {item.Kind}");
                        Console.WriteLine($"\t\t- Variation: {item.Variation}");
                        Console.WriteLine($"\t\t- SubKind: {item.SubKind}");
                        Console.WriteLine($"\t\t- X: {item.X}");
                        Console.WriteLine($"\t\t- Y: {item.Y}");
                        Console.WriteLine($"\t\t- HideModeKind: {item.HideModeKind}");
                    }
                    */
                }
            }
        }

        [TestMethod]
        public void MapTestTDXIO()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby Triple Deluxe\romfs\map\Step\Level1\Stage1\Step02.dat";
            const string OUT_PATH = "TDX_L1S1S02.bin";

            MapTDX map;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapTDX(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapTDX(reader);
        }

        [TestMethod]
        public void MapTestKPR()
        {
            const string PATH = @"D:\Game Dumps\Kirby Planet Robobot\romfs\map\Step";

            string[] maps = Directory.GetFiles(PATH, "*.dat", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                Console.WriteLine(maps[i].Remove(0, PATH.Length));
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapKPR map = new MapKPR(reader);
                    /*
                    Console.WriteLine($"\t- Width: {map.Width}");
                    Console.WriteLine($"\t- Height: {map.Height}");
                    */
                    Console.WriteLine($"\t- Background: {map.Background}");
                    Console.WriteLine($"\t- DecorSet: {map.DecorSet}");
                    Console.WriteLine($"\t- BGM: {map.BGM}");
                    Console.WriteLine($"\t- LightSet: {map.LightSet}");
                    Console.WriteLine($"\t- ScreenSplitKind: {map.ScreenSplitKind}");
                    Console.WriteLine($"\t- LanePairKind: {map.LanePairKind}");
                    Console.WriteLine($"\t- CustomRespawn: {map.CustomRespawn}");
                    Console.WriteLine($"\t- RespawnStepShift: {map.RespawnStepShift}");
                    Console.WriteLine($"\t- RespawnStartPortal: {map.RespawnStartPortal}");
                    Console.WriteLine($"\t- Unknown 6: {map.Unknown6}");
                    Console.WriteLine($"\t- Unknown 7: {map.Unknown7}");
                    Console.WriteLine($"\t- Unknown 8: {map.Unknown8}");
                    Console.WriteLine($"\t- Unknown 9: {map.Unknown9}");
                    Console.WriteLine($"\t- Unknown 10: {map.Unknown10}");
                    Console.WriteLine($"\t- BlankSpaceGridNum: {map.BlankSpaceGridNum}");
                    Console.WriteLine($"\t- Unknown 12: {map.Unknown12}");
                    Console.WriteLine($"\t- SystemLayout: {map.SystemLayout}");
                }
            }
        }

        [TestMethod]
        public void MapTestKSA()
        {
            /*
            {
                const string IN_PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\map\Step\Level3\Stage4\Step06.dat";
                const string OUT_PATH = "KSA_L3_S4_S06.dat";

                MapKSA map;
                using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                    map = new MapKSA(reader);

                using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
                using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                    map.Write(writer);

                using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                    map = new MapKSA(reader);

                return;
            }
            */

            const string PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\map\Step";

            string[] maps = Directory.GetFiles(PATH, "*.dat", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                Console.WriteLine(maps[i].Remove(0, PATH.Length));
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapKSA map = new MapKSA(reader);

                    Console.WriteLine("- General:");
                    Console.WriteLine($"  - Unknown1: {map.Unknown1}");
                    Console.WriteLine($"  - Unknown2: {map.Unknown2}");
                    Console.WriteLine($"  - LightSet: {map.LightSet}");
                    Console.WriteLine($"  - CustomRespawn: {map.CustomRespawn}");
                    Console.WriteLine($"  - RespawnStepShift: {map.RespawnStepShift}");
                    Console.WriteLine($"  - RespawnStartPortal: {map.RespawnStartPortal}");
                    Console.WriteLine($"  - BGCameraPos: {map.BGCameraPos}");
                    Console.WriteLine($"  - BGCameraMoveRate: {map.BGCameraMoveRate}");
                    Console.WriteLine($"  - BlankSpaceGridNum: {map.BlankSpaceGridNum}");
                    Console.WriteLine($"  - SystemLayout: {map.SystemLayout}");
                    Console.WriteLine($"  - BGMIndex: {map.UseAltBGM}");
                }
            }
        }

        //[TestMethod]
        public void MapTestGeneral()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\map\Step";

            string[] maps = Directory.GetFiles(PATH, "*.bin", SearchOption.AllDirectories);
            for (int i = 0; i < maps.Length; i++)
            {
                using (FileStream stream = new FileStream(maps[i], FileMode.Open, FileAccess.Read))
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    MapRtDL map = new MapRtDL(reader);

                    Console.WriteLine(maps[i].Remove(0, PATH.Length));
                    Console.WriteLine($"\t- BGM: {map.BGM}");
                    Console.WriteLine($"\t- CamPos: {map.BGCameraPos}");
                    Console.WriteLine($"\t- CamRot: {map.BGCameraRot}");
                    Console.WriteLine($"\t- FOVY: {map.BGCameraFOV}");
                    Console.WriteLine($"\t- MoveH: {map.BGCameraMoveRateH}");
                    Console.WriteLine($"\t- MoveV: {map.BGCameraMoveRateV}");
                    Console.WriteLine($"\t- SFX: {map.SFX}");
                    Console.WriteLine($"\t- Type: {map.Type}");
                    Console.WriteLine($"\t- Custom Respawn: {map.CustomRespawn}");
                    Console.WriteLine($"\t- Respawn Step Shift: {map.RespawnStepShift}");
                    Console.WriteLine($"\t- Respawn Start Portal: {map.RespawnStartPortal}");
                }
            }
        }

        [TestMethod]
        public void MapTestKF2()
        {
            const string IN_PATH = @"D:\Game Dumps\Kirby Fighters 2\romfs\map\Fight\Game\Main\DededeRing.bin";
            const string OUT_PATH = "KF2_DededeRing.bin";

            MapFighters map;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapFighters(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapFighters(reader);
        }

        [TestMethod]
        public void MapConvertTest()
        {
            const string IN_PATH = "Step01.dat";
            const string OUT_PATH = "Step01.bin";

            MapRtDL map;
            using (FileStream stream = new FileStream(IN_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);

            map.IsDeluxe = !map.IsDeluxe;

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);
        }

        [TestMethod]
        public void MapTest2Layer()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dreamland\DATA\files\map\step\level1\stage1\Step01.dat";
            const string OUT_PATH = @"C:\Users\firubii\Documents\Dolphin Emulator\Load\Riivolution\riivolution\test_lvl\map\step\level1\stage1\Step01.dat";

            MapRtDL map;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);

            CollisionTile[,] col = new CollisionTile[map.Collision[0].GetLength(0), map.Collision[0].GetLength(1)];
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    col[x, y] = new CollisionTile(LandGridShapeKind.Cube, LandGridProperty.None, 0, 0);
                }
            }
            map.Collision.Add(col);
            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);
        }

        void PrintMapInfo(MapRtDL map)
        {
            /*
            if (map.Bosses.Count > 0)
            {
                for (int i = 0; i < map.Bosses.Count; i++)
                {
                    Console.WriteLine($"\t- Boss {i}:");
                    Console.WriteLine($"\t\t- Kind: {map.Bosses[i].Kind}");
                    Console.WriteLine($"\t\t- Variation: {map.Bosses[i].Variation}");
                    Console.WriteLine($"\t\t- Level: {map.Bosses[i].Level}");
                    Console.WriteLine($"\t\t- TerrainGroup: {map.Bosses[i].TerrainGroup}");
                    Console.WriteLine($"\t\t- Has Super Ability: {map.Bosses[i].HasSuperAbility}");
                    Console.WriteLine($"\t\t- X: {map.Bosses[i].X}");
                    Console.WriteLine($"\t\t- Y: {map.Bosses[i].Y}");
                    Console.WriteLine($"\t\t- Unknown: {map.Bosses[i].Unknown}");
                }
            }
            */
            /*
            if (map.Enemies.Count > 0)
            {
                for (int i = 0; i < map.Enemies.Count; i++)
                {
                    Console.WriteLine($"\t- Enemy {i}:");
                    Console.WriteLine($"\t\t- Kind: {map.Enemies[i].Kind}");
                    Console.WriteLine($"\t\t- Variation: {map.Enemies[i].Variation}");
                    Console.WriteLine($"\t\t- Level: {map.Enemies[i].Level}");
                    Console.WriteLine($"\t\t- Dir Type: {map.Enemies[i].Direction}");
                    Console.WriteLine($"\t\t- Another Dimension Size: {map.Enemies[i].AnotherDimensionSize}");
                    Console.WriteLine($"\t\t- Extra Mode Size: {map.Enemies[i].ExtraModeSize}");
                    Console.WriteLine($"\t\t- Terrain Group: {map.Enemies[i].TerrainGroup}");
                    Console.WriteLine($"\t\t- Has Super Ability: {map.Enemies[i].HasSuperAbility}");
                    Console.WriteLine($"\t\t- X: {map.Enemies[i].X}");
                    Console.WriteLine($"\t\t- Y: {map.Enemies[i].Y}");
                    Console.WriteLine($"\t\t- Is Normal Only: {map.Enemies[i].IsNormalOnly}");
                    Console.WriteLine($"\t\t- Is EX Only: {map.Enemies[i].IsEXOnly}");
                }
            }
            */
            /*
            if (map.DecorationObjects.Count > 0)
            {
                for (int l = 0; l < map.DecorationObjects.Count; l++)
                {
                    Console.WriteLine($"\t- Decor Object {l}:");
                    Console.WriteLine($"\t\t- Kind: " + map.DecorationObjects[l].Kind);
                    Console.WriteLine($"\t\t- X1: " + map.DecorationObjects[l].X1);
                    Console.WriteLine($"\t\t- Y1: " + map.DecorationObjects[l].Y1);
                    Console.WriteLine($"\t\t- X2: " + map.DecorationObjects[l].X2);
                    Console.WriteLine($"\t\t- Y2: " + map.DecorationObjects[l].Y2);
                    Console.WriteLine($"\t\t- Color 1: " + map.DecorationObjects[l].Color1);
                    Console.WriteLine($"\t\t- Color 2: " + map.DecorationObjects[l].Color2);
                    Console.WriteLine($"\t\t- Radius: " + map.DecorationObjects[l].Radius);
                    Console.WriteLine($"\t\t- Terrain Group: " + map.DecorationObjects[l].TerrainGroup);
                    Console.WriteLine($"\t\t- Param 1: " + map.DecorationObjects[l].Param1);
                    Console.WriteLine($"\t\t- Param 2: " + map.DecorationObjects[l].Param2);
                    Console.WriteLine($"\t\t- Param 3: " + map.DecorationObjects[l].Param3);
                    Console.WriteLine($"\t\t- Param 4: " + map.DecorationObjects[l].Param4);
                    Console.WriteLine($"\t\t- Param 5: " + map.DecorationObjects[l].Param5);
                    Console.WriteLine($"\t\t- Param 6: " + map.DecorationObjects[l].Param6);
                    Console.WriteLine($"\t\t- Param 7: " + map.DecorationObjects[l].Param7);
                    Console.WriteLine($"\t\t- Param 8: " + map.DecorationObjects[l].Param8);
                }
            }
            */
            /*
            if (map.Type != MapRtDL.MapType.Normal)
            {
                Console.WriteLine($"{maps[i].Remove(0, PATH.Length)} is of type {map.Type}");
            }
            */
            
            if (map.CollisionMoveGroups.Count(x => x.IsValid) > 0)
            {
                for (int j = 0; j < map.CollisionMoveGroups.Length; j++)
                {
                    var group = map.CollisionMoveGroups[j];
                    if (group.IsValid)
                    {
                        Console.WriteLine($"\t- Group {j}:");
                        Console.WriteLine($"\t\t- X: {group.X}");
                        Console.WriteLine($"\t\t- Y: {group.Y}");
                        Console.WriteLine($"\t\t- Width: {group.Width}");
                        Console.WriteLine($"\t\t- Height: {group.Height}");

                        if (group.Action.IsValid)
                        {
                            Console.WriteLine($"\t\t- Action:");
                            Console.WriteLine($"\t\t\t- SignalNo: {group.Action.SignalNo}");
                            Console.WriteLine($"\t\t\t- Param 1: {group.Action.Param1}");
                            Console.WriteLine($"\t\t\t- Param 2: {group.Action.Param2}");
                            Console.WriteLine($"\t\t\t- Start Immediately: {group.Action.StartImmediately}");

                            var orders = group.Action.Orders;
                            for (int e = 0; e < orders.Count; e++)
                            {
                                var order = orders[e];
                                Console.WriteLine($"\t\t\t- Order {e}:");
                                Console.WriteLine($"\t\t\t\t- Direction: {order.Direction}");
                                Console.WriteLine($"\t\t\t\t- Distance: {order.Distance}");
                                Console.WriteLine($"\t\t\t\t- WaitFrame: {order.WaitFrame}");

                                if (map.IsDeluxe)
                                    Console.WriteLine($"\t\t\t\t- DX Unknown 1: {order.DXUnknown1}");

                                Console.WriteLine($"\t\t\t\t- Scalar: {order.Scalar}");

                                if (map.IsDeluxe)
                                    Console.WriteLine($"\t\t\t\t- DX Unknown 2: {order.DXUnknown2}");

                                Console.WriteLine($"\t\t\t\t- Frame: {order.Frame}");

                                if (map.IsDeluxe)
                                    Console.WriteLine($"\t\t\t\t- DX Unknown 3: {order.DXUnknown3}");

                                Console.WriteLine($"\t\t\t\t- Is End: {order.IsEnd}");
                                Console.WriteLine($"\t\t\t\t- GoTo: {order.GoTo}");
                                Console.WriteLine($"\t\t\t\t- SEStart: {order.SEStart}");
                                Console.WriteLine($"\t\t\t\t- SEMove: {order.SEMove}");
                                Console.WriteLine($"\t\t\t\t- SEStop: {order.SEStop}");
                                Console.WriteLine($"\t\t\t\t- QuakeKindStart: {order.QuakeKindStart}");
                                Console.WriteLine($"\t\t\t\t- QuakeKindMove: {order.QuakeKindMove}");
                                Console.WriteLine($"\t\t\t\t- QuakeKindStop: {order.QuakeKindStop}");
                                Console.WriteLine($"\t\t\t\t- VibrationStart: {order.VibrationStart}");
                                Console.WriteLine($"\t\t\t\t- VibrationMove: {order.VibrationMove}");
                                Console.WriteLine($"\t\t\t\t- VibrationStop: {order.VibrationStop}");
                                Console.WriteLine($"\t\t\t\t- Unknown 0x13: {order.Unknown_0x13}");
                                Console.WriteLine($"\t\t\t\t- Pattern: {order.Pattern}");
                                Console.WriteLine($"\t\t\t\t- MoveRate: {order.MoveRate}");
                                Console.WriteLine($"\t\t\t\t- Dir5 X: {order.Direction5_X}");
                                Console.WriteLine($"\t\t\t\t- Dir5 Y: {order.Direction5_Y}");
                            }
                        }
                    }
                }
            }
            
        }

        [TestMethod]
        public void CinemoTest()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dream Land Deluxe\romfs\cinemoparam\Yy\Tps\HeroActionParam.cndbin";
            const string OUT_PATH = "HeroActionParam_outtest.cndbin";

            Cinemo cnd;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                cnd = new Cinemo(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                cnd.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                cnd = new Cinemo(reader);
        }

        [TestMethod]
        public void CinemoTestKSA()
        {
            const string PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\map\Step\Level4\Stage1\Step01.cndbin";
            const string OUT_PATH = "KSA_L4_S1_S01.cndbin";

            CinemoKSA cnd;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                cnd = new CinemoKSA(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                cnd.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                cnd = new CinemoKSA(reader);
        }

        [TestMethod]
        public void FDGTestV2()
        {
            const string PATH = @"D:\Game Dumps\Kirby's Return to Dreamland\DATA\files\fdg\Archive.dat";
            const string OUT_PATH = "RTDL_FDG.dat";

            FDG fdg;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                fdg = new FDG(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                fdg.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                fdg = new FDG(reader);
        }

        [TestMethod]
        public void FDGTestV3()
        {
            const string PATH = @"D:\Game Dumps\Kirby and the Forgotten Land\romfs\fdg\Archive.dat";
            const string OUT_PATH = "KatFL_FDG.dat";

            FDG fdg;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                fdg = new FDG(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                fdg.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                fdg = new FDG(reader);
        }

        [TestMethod]
        public void FilterTest()
        {
            const string PATH = @"D:\Game Dumps\Kirby and the Forgotten Land\romfs\msg\Kirby15\US_English\Filter.bin";
            const string OUT_PATH = "KatFL_Filter.bin";

            MsgFilter filter;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                filter = new MsgFilter(reader);

            for (int i = 0; i < filter.Filters.Count; i++)
            {
                Console.WriteLine($"- {filter.Filters[i].Font}");
                Console.WriteLine($"\t- {filter.Filters[i].Characters}");
            }

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                filter.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                filter = new MsgFilter(reader);

            Console.WriteLine("-------------");

            for (int i = 0; i < filter.Filters.Count; i++)
            {
                Console.WriteLine($"- {filter.Filters[i].Font}");
                Console.WriteLine($"\t- {filter.Filters[i].Characters}");
            }
        }

        [TestMethod]
        public void Map3DTest()
        {
            const string PATH = @"D:\Game Dumps\Kirby Star Allies\romfs\map3d\LvMap\LvMap3\LvMap3Bg\LvMap3\TopL.bin";
            const string OUT_PATH = "LvMap3_test.bin";

            Map3D map3d;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map3d = new Map3D(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map3d.Write(writer);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map3d = new Map3D(reader);
        }

        [TestMethod]
        public void Map3DRumbleTest()
        {
            const string PATH = @"";
            const string OUT_PATH = @"";

            Map3DRumble map;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new Map3DRumble(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);
        }

        [TestMethod]
        public void MapKBBTest()
        {
            const string PATH = @"";
            const string OUT_PATH = @"";

            MapKBB map;
            using (FileStream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapKBB(reader);

            using (FileStream stream = new FileStream(OUT_PATH, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);
        }
    }
}