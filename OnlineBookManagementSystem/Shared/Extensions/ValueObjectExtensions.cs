using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Shared.Extensions;

public static class ValueObjectExtensions
{
    // Money extensions
    public static decimal ToDecimal(this Money money) => money.Amount;
    
    public static Money ToMoney(this decimal value) => Money.FromDecimal(value);
    
    // ISBN extensions
    public static string ToIsbnString(this ISBN isbn) => isbn.Value;
    
    public static ISBN ToIsbn(this string value) => ISBN.Create(value);
    
    // Address extensions
    public static string ToAddressString(this Address? address) => address?.ToString() ?? string.Empty;
    
    // Enum extensions
    public static string ToStatusString(this OrderStatus status) => status.ToString();
    
    public static string ToStatusString(this PaymentStatus status) => status.ToString();
    
    public static string ToStatusString(this ReviewStatus status) => status.ToString();
}
