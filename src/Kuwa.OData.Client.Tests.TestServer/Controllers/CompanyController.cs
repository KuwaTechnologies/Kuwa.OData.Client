using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Kuwa.OData.Client.Tests.TestServer.Infrastructure;
using Kuwa.OData.Client.Tests.TestServer.Data;
using Kuwa.OData.Client.Tests.TestServer.Models;

namespace Kuwa.OData.Client.Tests.TestServer.Controllers;

/// <summary>The <c>Company</c> singleton.</summary>
public class CompanyController(NorthwindData data) : NorthwindController(data)
{
	[NorthwindQuery]
	public IActionResult Get() => Ok(Data.Company);

	public IActionResult Patch([FromBody] Delta<Company> delta)
	{
		if (delta is null)
		{
			return BadRequest();
		}

		Data.Mutate(() => delta.Patch(Data.Company));
		return Updated(Data.Company);
	}
}
