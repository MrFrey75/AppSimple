using AppSimple.Core.Models.Requests;
using AppSimple.WebApp.Models;
using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for notes and tags pages.</summary>
[Authorize]
[Route("notes")]
public sealed class NotesController : Controller
{
    private readonly IApiClient _api;
    private readonly ILogger<NotesController> _logger;

    /// <summary>Initializes a new instance of <see cref="NotesController"/>.</summary>
    public NotesController(IApiClient api, ILogger<NotesController> logger)
    {
        _api    = api;
        _logger = logger;
    }

    private string? GetToken() => User.FindFirstValue("jwt_token");

    // ── Notes ─────────────────────────────────────────────────────────────

    /// <summary>Lists all notes for the current user.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var notes = await _api.GetMyNotesAsync(token);
        return View(new NoteListViewModel { Notes = notes });
    }

    /// <summary>Displays a single note with tag management.</summary>
    [HttpGet("{uid:guid}")]
    public async Task<IActionResult> Detail(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var note = await _api.GetNoteAsync(token, uid);
        if (note is null)
        {
            TempData["Error"] = "Note not found.";
            return RedirectToAction(nameof(Index));
        }

        var allTags = await _api.GetMyTagsAsync(token);
        return View(new NoteDetailViewModel { Note = note, AllTags = allTags });
    }

    /// <summary>Displays the create note form.</summary>
    [HttpGet("create")]
    public IActionResult Create() => View(new CreateNoteViewModel());

    /// <summary>Processes the create note form.</summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNoteViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var result = await _api.CreateNoteAsync(token, new CreateNoteRequest
        {
            Title   = model.Title ?? string.Empty,
            Content = model.Content,
        });

        if (result is null)
        {
            TempData["Error"] = "Failed to create note.";
            return View(model);
        }

        _logger.LogInformation("User '{User}' created note {Uid}", User.Identity?.Name, result.Uid);
        TempData["Success"] = "Note created.";
        return RedirectToAction(nameof(Detail), new { uid = result.Uid });
    }

    /// <summary>Displays the edit note form.</summary>
    [HttpGet("{uid:guid}/edit")]
    public async Task<IActionResult> Edit(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var note = await _api.GetNoteAsync(token, uid);
        if (note is null) return RedirectToAction(nameof(Index));

        return View(new EditNoteViewModel { Uid = uid, Title = note.Title, Content = note.Content });
    }

    /// <summary>Processes the edit note form.</summary>
    [HttpPost("{uid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid uid, EditNoteViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var result = await _api.UpdateNoteAsync(token, uid, new UpdateNoteRequest
        {
            Title   = model.Title,
            Content = model.Content,
        });

        if (result is null)
        {
            TempData["Error"] = "Failed to update note.";
            return View(model);
        }

        TempData["Success"] = "Note updated.";
        return RedirectToAction(nameof(Detail), new { uid });
    }

    /// <summary>Deletes a note.</summary>
    [HttpPost("{uid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeleteNoteAsync(token, uid);
        TempData["Success"] = "Note deleted.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Adds a tag to a note.</summary>
    [HttpPost("{noteUid:guid}/tags/{tagUid:guid}/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(Guid noteUid, Guid tagUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.AddTagToNoteAsync(token, noteUid, tagUid);
        return RedirectToAction(nameof(Detail), new { uid = noteUid });
    }

    /// <summary>Removes a tag from a note.</summary>
    [HttpPost("{noteUid:guid}/tags/{tagUid:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(Guid noteUid, Guid tagUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.RemoveTagFromNoteAsync(token, noteUid, tagUid);
        return RedirectToAction(nameof(Detail), new { uid = noteUid });
    }

    // ── Tags ──────────────────────────────────────────────────────────────

    /// <summary>Lists all tags for the current user.</summary>
    [HttpGet("~/tags")]
    public async Task<IActionResult> Tags()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var tags = await _api.GetMyTagsAsync(token);
        return View(new TagListViewModel { Tags = tags });
    }

    /// <summary>Displays the create tag form.</summary>
    [HttpGet("~/tags/create")]
    public IActionResult CreateTag() => View(new CreateTagViewModel());

    /// <summary>Processes the create tag form.</summary>
    [HttpPost("~/tags/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTag(CreateTagViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var result = await _api.CreateTagAsync(token, new CreateTagRequest
        {
            Name        = model.Name,
            Description = model.Description,
            Color       = model.Color,
        });

        if (result is null)
        {
            TempData["Error"] = "Failed to create tag.";
            return View(model);
        }

        TempData["Success"] = $"Tag \"{result.Name}\" created.";
        return RedirectToAction(nameof(Tags));
    }

    /// <summary>Displays the edit tag form.</summary>
    [HttpGet("~/tags/{uid:guid}/edit")]
    public async Task<IActionResult> EditTag(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var tag = await _api.GetTagAsync(token, uid);
        if (tag is null) return RedirectToAction(nameof(Tags));

        return View(new EditTagViewModel
        {
            Uid         = uid,
            Name        = tag.Name,
            Description = tag.Description,
            Color       = tag.Color ?? "#CCCCCC",
        });
    }

    /// <summary>Processes the edit tag form.</summary>
    [HttpPost("~/tags/{uid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTag(Guid uid, EditTagViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.UpdateTagAsync(token, uid, new UpdateTagRequest
        {
            Name        = model.Name,
            Description = model.Description,
            Color       = model.Color,
        });

        TempData["Success"] = "Tag updated.";
        return RedirectToAction(nameof(Tags));
    }

    /// <summary>Deletes a tag.</summary>
    [HttpPost("~/tags/{uid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTag(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeleteTagAsync(token, uid);
        TempData["Success"] = "Tag deleted.";
        return RedirectToAction(nameof(Tags));
    }
}
