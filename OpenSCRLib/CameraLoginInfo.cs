using System.Security;

namespace OpenSCRLib
{
    public class CameraLoginInfo
    {
        public int Id { get; set; }

        public bool IsLoggedIn { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        // 確認しやすくするため、ToStringメソッドをオーバーライドする
        public override string ToString()
        {
            return $"CameraLoginInfo(Id={this.Id}, IsLoggedIn*{this.IsLoggedIn}, Name={this.Name}, Password={this.Password})";
        }
    }
}