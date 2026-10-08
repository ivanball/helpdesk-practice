namespace Helpdesk.ArchitectureTests;

public class PlaceholderTests
{
    [Fact]
    public void ModuleInfo_Name_IsExpected()
    {
        Assert.Equal("SharedKernel", Helpdesk.SharedKernel.ModuleInfo.Name);
    }
}
