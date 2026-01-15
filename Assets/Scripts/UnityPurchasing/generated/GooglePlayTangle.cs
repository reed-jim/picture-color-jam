// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("DY6Aj78NjoWNDY6OjzMXqw5eW79B1T1koT6vXcSJnkixgXKU42nKlgZOVwrdFvykCsKyTkzAng94P/jvaYtgfVME+5M+GMI5bKGmVPRe92LArFQu8bcqySjJmjusUZ9ZyLi45EfJj1rRRWPuLBxWa07VCuwp+BTFvw2Orb+CiYalCccJeIKOjo6Kj4wFk04v08Z4BwgkOwtRWWrL/UKBgWhnwMaHNQY7NvSftrjiPLOdt7vEjdyd2qZaYunTatacKn3f8TlKhPFIvdkH7CGstjuD3W8AtLaTWvPsHQWYWx53sUuP2Dtft1YXyOsmwGsCCLDgJs2E6tVEAcMcq1qRMpW0pQIn4hw9XTdtFuSM95SYs/ITrCUIcAJdnF0Zl/xJRo2Mjo+O");
        private static int[] order = new int[] { 1,2,7,6,12,10,7,11,12,13,12,12,12,13,14 };
        private static int key = 143;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
