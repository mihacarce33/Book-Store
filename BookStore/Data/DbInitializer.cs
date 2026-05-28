using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedCatalogAsync(context);
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        if (await context.Books.AnyAsync())
        {
            return;
        }

        var genres = new[]
        {
            new Genre { Name = "Classic Literature" },
            new Genre { Name = "Romance" },
            new Genre { Name = "Historical Fiction" },
            new Genre { Name = "Philosophy" },
            new Genre { Name = "Gothic" },
            new Genre { Name = "American Literature" }
        };
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();

        var genreByName = await context.Genres.ToDictionaryAsync(g => g.Name);

        var authors = new[]
        {
            new Author
            {
                Name = "Jane Austen",
                Biography = "English novelist known for wit, social observation, and masterpieces of romantic fiction in the Regency era.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/cd/CassandraAusten-JaneAusten(c.1810)_hires.jpg/440px-CassandraAusten-JaneAusten(c.1810)_hires.jpg"
            },
            new Author
            {
                Name = "Charles Dickens",
                Biography = "Victorian author whose sprawling novels gave voice to London's poor and the contradictions of industrial society.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/aa/Dickens_Gurney_head.jpg/440px-Dickens_Gurney_head.jpg"
            },
            new Author
            {
                Name = "Virginia Woolf",
                Biography = "Modernist writer and critic; her stream-of-consciousness prose reshaped twentieth-century English letters.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/George_Charles_Beresford_-_Virginia_Woolf_in_1902_-_Restoration.jpg/440px-George_Charles_Beresford_-_Virginia_Woolf_in_1902_-_Restoration.jpg"
            },
            new Author
            {
                Name = "F. Scott Fitzgerald",
                Biography = "American novelist of the Jazz Age, chronicler of ambition, beauty, and the cost of dreams.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/F_Scott_Fitzgerald_1921.jpg/440px-F_Scott_Fitzgerald_1921.jpg"
            },
            new Author
            {
                Name = "Leo Tolstoy",
                Biography = "Russian master of epic fiction and moral inquiry; his works remain monuments of world literature.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c6/L.N.Tolstoy_Prokudin-Gorsky.jpg/440px-L.N.Tolstoy_Prokudin-Gorsky.jpg"
            },
            new Author
            {
                Name = "Charlotte Brontë",
                Biography = "Author of fierce, intimate fiction; her heroines challenged Victorian notions of womanhood and desire.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9c/Charlotte_Bront%C3%AB_1850.jpg/440px-Charlotte_Bront%C3%AB_1850.jpg"
            }
        };
        context.Authors.AddRange(authors);
        await context.SaveChangesAsync();

        var authorByName = await context.Authors.ToDictionaryAsync(a => a.Name);

        var books = new List<Book>
        {
            new()
            {
                Title = "Pride and Prejudice",
                AuthorId = authorByName["Jane Austen"].Id,
                GenreId = genreByName["Romance"].Id,
                Price = 24.50f,
                Stock = 12,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780141439518-L.jpg",
                Description = "A beloved comedy of manners in which Elizabeth Bennet and Mr. Darcy overcome pride, prejudice, and society's expectations."
            },
            new()
            {
                Title = "Emma",
                AuthorId = authorByName["Jane Austen"].Id,
                GenreId = genreByName["Romance"].Id,
                Price = 22.00f,
                Stock = 8,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780141439587-L.jpg",
                Description = "Austen's portrait of a well-meaning matchmaker who must learn the limits of her own judgment."
            },
            new()
            {
                Title = "Great Expectations",
                AuthorId = authorByName["Charles Dickens"].Id,
                GenreId = genreByName["Classic Literature"].Id,
                Price = 19.95f,
                Stock = 15,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780141439563-L.jpg",
                Description = "The orphan Pip rises through Victorian society, only to discover the true source of his fortune and identity."
            },
            new()
            {
                Title = "Bleak House",
                AuthorId = authorByName["Charles Dickens"].Id,
                GenreId = genreByName["Historical Fiction"].Id,
                Price = 28.00f,
                Stock = 6,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780141439709-L.jpg",
                Description = "A vast, fog-drenched indictment of the law's cruelty, told through interconnected lives across London."
            },
            new()
            {
                Title = "Mrs Dalloway",
                AuthorId = authorByName["Virginia Woolf"].Id,
                GenreId = genreByName["Classic Literature"].Id,
                Price = 18.50f,
                Stock = 10,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780156030359-L.jpg",
                Description = "A single day in London unfolds in luminous interior monologue, memory, and the passage of time."
            },
            new()
            {
                Title = "To the Lighthouse",
                AuthorId = authorByName["Virginia Woolf"].Id,
                GenreId = genreByName["Classic Literature"].Id,
                Price = 17.25f,
                Stock = 7,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780156904152-L.jpg",
                Description = "Woolf's meditation on art, loss, and the Ramsay family's summers by the sea."
            },
            new()
            {
                Title = "The Great Gatsby",
                AuthorId = authorByName["F. Scott Fitzgerald"].Id,
                GenreId = genreByName["American Literature"].Id,
                Price = 16.00f,
                Stock = 20,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780743273565-L.jpg",
                Description = "Nick Carraway bears witness to Jay Gatsby's doomed pursuit of wealth and an impossible love."
            },
            new()
            {
                Title = "Tender Is the Night",
                AuthorId = authorByName["F. Scott Fitzgerald"].Id,
                GenreId = genreByName["American Literature"].Id,
                Price = 21.00f,
                Stock = 5,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780684801544-L.jpg",
                Description = "A glittering Riviera setting frames the unraveling of a psychiatrist, his wife, and a young heiress."
            },
            new()
            {
                Title = "War and Peace",
                AuthorId = authorByName["Leo Tolstoy"].Id,
                GenreId = genreByName["Historical Fiction"].Id,
                Price = 34.00f,
                Stock = 9,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780199232765-L.jpg",
                Description = "Tolstoy's epic of Russian society during the Napoleonic wars — love, duty, and the search for meaning."
            },
            new()
            {
                Title = "Anna Karenina",
                AuthorId = authorByName["Leo Tolstoy"].Id,
                GenreId = genreByName["Romance"].Id,
                Price = 26.50f,
                Stock = 11,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780143035008-L.jpg",
                Description = "A tragedy of passion and hypocrisy set against the moral landscape of imperial Russia."
            },
            new()
            {
                Title = "Jane Eyre",
                AuthorId = authorByName["Charlotte Brontë"].Id,
                GenreId = genreByName["Gothic"].Id,
                Price = 18.75f,
                Stock = 14,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780141441146-L.jpg",
                Description = "An orphan governess refuses to compromise her integrity for love, mystery, or social advancement."
            },
            new()
            {
                Title = "Villette",
                AuthorId = authorByName["Charlotte Brontë"].Id,
                GenreId = genreByName["Gothic"].Id,
                Price = 20.25f,
                Stock = 4,
                ImageURL = "https://covers.openlibrary.org/b/isbn/9780140434798-L.jpg",
                Description = "Lucy Snowe teaches abroad in a foreign school, confronting solitude, desire, and self-reliance."
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }
}
