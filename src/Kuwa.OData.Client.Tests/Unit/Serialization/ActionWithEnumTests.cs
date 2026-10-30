using System.Text.Json;
using Kuwa.OData.Client.Serialization;
using Kuwa.OData.Client.Tests.Infrastructure;

namespace Kuwa.OData.Client.Tests.Unit.Serialization;

public sealed class ActionWithEnumTests
{
	private enum Rank
	{
		First,
		Second,
		Third,
	}

	[Fact]
	public async Task BoundActionWithEnumParameter()
	{
		var writer = new RequestWriter(MetadataClientFactory.CreateSession("ActionWithEnum.xml"), null);
		var request = await writer.CreateActionRequestAsync("Entity/Models.MakeFromParam", "MakeFromParam", "Models.Entity",
			new Dictionary<string, object?> { ["Name"] = "Entity Name", ["Rank"] = Rank.Second }, false);
		request.Method.Should().Be("POST");
		var json = JsonDocument.Parse(await request.RequestMessage.Content!.ReadAsStringAsync()).RootElement;
		json.GetProperty("Name").GetString().Should().Be("Entity Name");
		json.GetProperty("Rank").GetString().Should().Be("Second");
	}
}
