using PostService.CommonTypes;
namespace PostService.Dtos;
/// <summary>Вхідні дані нового відправлення. Вартість розраховує сервер.</summary>
public class PostingPostDto
{
    /// <summary>Відправник та його адреса.</summary>
    public string From { get; set; } = "";
    /// <summary>Отримувач та його адреса.</summary>
    public string To { get; set; } = "";
    /// <summary>Вміст відправлення.</summary>
    public string Content { get; set; } = "";
    /// <summary>Спосіб доставки 0 відділення 1 курʼєр 2 експрес.</summary>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Вага у кілограмах.</summary>
    public float Weight { get; set; }
    /// <summary>Ширина у сантиметрах.</summary>
    public float Width { get; set; }
    /// <summary>Висота у сантиметрах.</summary>
    public float Height { get; set; }
    /// <summary>Глибина у сантиметрах.</summary>
    public float Depth { get; set; }
    /// <summary>Оголошена цінність у гривнях або null.</summary>
    public float? Value { get; set; }
    /// <summary>Додатковий опис або null.</summary>
    public string? Description { get; set; }
}
/// <summary>Нові значення полів наявного відправлення.</summary>
public class PostingPutDto : PostingPostDto { }
/// <summary>Повні дані збереженого відправлення.</summary>
public class PostingGetDto : PostingPostDto
{
    /// <summary>Ідентифікатор, створений базою даних.</summary>
    public int Id { get; set; }
    /// <summary>Розрахована сервером вартість у гривнях.</summary>
    public float Price { get; set; }
    /// <summary>Дата та час створення за UTC.</summary>
    public DateTime CreatedAt { get; set; }
}
