using System.Net.Http.Json;

// Esperar 2 segundos para asegurar que la API haya arrancado en Docker
await Task.Delay(2000);

var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5178/");

try
{
    Console.WriteLine("--- INICIANDO PRUEBAS DE LA API ---");

    // 1. GET: Consultar Stock inicial
    //var checkResponse = await client.GetFromJsonAsync<StockResponse>("api/inventory/check/P001/3");
   // Console.WriteLine($"[1. GET] Check Stock P001 -> Disponible: {checkResponse?.Available}");

    // 2. POST: Crear una orden (descuenta stock)
    var orderRequest = new { ProductId = "P001", Quantity = 5 };
    var postResponse = await client.PostAsJsonAsync("api/inventory/order", orderRequest);
    var orderResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse>();
    Console.WriteLine($"[2. POST] Create Order P001 -> Mensaje: '{orderResult?.Message}'");

    // 3. PUT: Actualizar el stock del producto
    var updateRequest = new { ProductId = "P001", NewQuantity = 500 };
    var putResponse = await client.PutAsJsonAsync("api/inventory/update", updateRequest);
    var updateResult = await putResponse.Content.ReadFromJsonAsync<ApiResponse>();
    Console.WriteLine($"[3. PUT] Update Stock P001 -> Mensaje: '{updateResult?.Message}'");

    // 4. DELETE: Eliminar un producto
    var deleteRequest = new { ProductId = "P002" };
    var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/inventory/delete")
    {
        Content = JsonContent.Create(deleteRequest)
    };
    var deleteHttpResponse = await client.SendAsync(requestMessage);
    var deleteResult = await deleteHttpResponse.Content.ReadFromJsonAsync<ApiResponse>();
    Console.WriteLine($"[4. DELETE] Delete Product P002 -> Mensaje: '{deleteResult?.Message}'");

    Console.WriteLine("--- PRUEBAS FINALIZADAS CON ÉXITO ---");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Falló la comunicación: {ex.Message}");
}

// Modelos para leer las respuestas de la API
public class StockResponse
{
    public string ProductId { get; set; } = string.Empty;
    public bool Available { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}