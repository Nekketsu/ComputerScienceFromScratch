using System.Text;
using Xunit.Abstractions;

namespace NESEmulator.Tests;

public class NESEmulatorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private const string TestFolder = "Tests";

    [Fact]
    public void TestNesTest()
    {
        // Create machinery that we are testing
        var rom = new Rom(Path.Combine(TestFolder, "nestest", "nestest.nes"));
        var ppu = new Ppu(rom);
        var cpu = new Cpu(ppu, rom)
        {
            // Set up tests
            PC = 0xC000 // Special starting location for tests
        };

        var correctLines = File.ReadAllLines(Path.Combine(TestFolder, "nestest", "nestest.log"));
        var logLine = 1;
        // Check every line of the log against our own produced logs
        while (logLine < 5260) // Go until first unofficial opcode test
        {
            var ourLine = cpu.Log();
            var correctLine = correctLines[logLine - 1];
            Assert.True(correctLine[0..14] == ourLine[0..14], $"PC/OpCode doesn't match at line {logLine}");
            Assert.True(correctLine[48..73] == ourLine[48..73], $"Registers don't match at line {logLine}");
            cpu.Step();
            logLine++;
        }
    }

    [Theory]
    [InlineData("01-basics.nes")]
    [InlineData("02-implied.nes")]
    [InlineData("10-branches.nes")]
    [InlineData("11-stack.nes")]
    [InlineData("12-jmp_jsr.nes")]
    [InlineData("13-rts.nes")]
    [InlineData("14-rti.nes")]
    [InlineData("15-brk.nes")]
    [InlineData("16-special.nes")]
    public void TestBlarggInstrTestV5(string test)
    {
        var testName = test.Split('-', '.')[1];

        // Create machinery that we are testing
        var rom = new Rom(Path.Combine(TestFolder, "instr_test-v5", "rom_singles", test));
        var ppu = new Ppu(rom);
        var cpu = new Cpu(ppu, rom);

        // Tests run as long as 0x6000 is 80, an then 0x6000 is result code; 0 means success
        rom.PrgRam[0] = 0x80;
        while (rom.PrgRam[0] == 0x80) // Go until first unofficial OpCode test
        {
            cpu.Step();
        }
        Assert.True(0 == rom.PrgRam[0], $"Result code of {testName} test is {rom.PrgRam[0]} not 0");
        var message = Encoding.UTF8.GetString(rom.PrgRam[4..]);
        var index = message.IndexOf('\0'); // Message ends with null terminator
        if (index >= 0)
        {
            message = message[..index];
        }
        output.WriteLine(message);
    }
}
