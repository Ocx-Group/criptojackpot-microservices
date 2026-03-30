namespace CryptoJackpot.Lottery.Domain.Enums;

/// <summary>
/// Tipos de lotería disponibles
/// </summary>
public enum LotteryType
{
    Standard,       // Lotería regular
    Instant,        // Resultados instantáneos
    Progressive,    // Premio acumulativo
    Pick3 = 5       // Juego de 3 dígitos (000-999), el usuario elige 3 números
}

