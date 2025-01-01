namespace FlowNetFramework.Commons.Constants;

public static class ResponseMessage
{
    public const string Succeed = "İşlem başarılı.";
    public const string NotFound = "Kayıt bulunamadı.";
    public const string ServerError = "Bir hata oluştu.";
    public const string OperationCancelled = "İşlem iptal edildi.";
    public const string InvalidRequest = "Geçersiz İstek.";
    public const string Failed = "İşlem yapılırken hata oluştu.";
    public const string UnauthorizedError = "Yetkisiz giriş. Lütfen kullanıcı adı veya şifrenizi kontrol ediniz. ";
    public const string Conflict = "Eklemeye çalıştığınız veri zaten sistemde kayıtlıdır.";
    public const string Forbidden = "İlişkili kayıt olduğu için bu işlemin yapılması engellenmiştir.";

}
