using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace castYourDotNets.Models;

public class Verse_Vault
{
    public enum Scripture { BookOfMormon, OldTestament, NewTestament, DoctrineAndCovenants }
    [Required]
    public Guid id { get; set; } = Guid.NewGuid();
    [Required]
    public Scripture scripture { get; set; }
    [Required]
    public required string book { get; set; }
    [Required]

    public int Chapter
    { get; set; }
    [Required]
    public int VerseInt { get; set; }
    [Required]
    public required string Verse_Refrence { get; set; }
    [Required]
    public required string VerseText { get; set; }


}
