﻿#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.Admin;

[ApiController]
[Route("admin/report/{id:int}")]
public class AdminReportController : ControllerBase
{
    private readonly Database database;

    public AdminReportController(Database database)
    {
        this.database = database;
    }

    [HttpGet("remove")]
    public async Task<IActionResult> DeleteReport([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.StatusCode(403, "");

        GriefReport? report = await this.database.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
        if (report == null) return this.NotFound();

        List<string> hashes = new()
        {
            report.JpegHash,
            report.GriefStateHash,
            report.InitialStateHash,
        };
        foreach (string hash in hashes)
        {
            if (System.IO.File.Exists($"png{Path.DirectorySeparatorChar}{hash}"))
            {
                System.IO.File.Delete($"png{Path.DirectorySeparatorChar}{hash}");
            }
            if (System.IO.File.Exists($"r{Path.DirectorySeparatorChar}{hash}"))
            {
                System.IO.File.Delete($"r{Path.DirectorySeparatorChar}{hash}");
            }
        }
        this.database.Reports.Remove(report);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/reports/0");
    }
}