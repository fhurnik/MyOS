using MyOS.Modules.Storage.Domain.AllowedFileTypes;

namespace MyOS.UnitTests.Common
{
    // AllowedFileType is a config entity seeded via migration with no construction API
    // (only EF's private ctor). Tests materialize it via reflection.
    internal static class StorageTestData
    {
        public static AllowedFileType AllowedType(
            bool isActive = true, string extension = "png", string contentType = "image/png")
        {
            var type = (AllowedFileType)Activator.CreateInstance(typeof(AllowedFileType), nonPublic: true)!;
            SetProperty(type, "Extension", extension);
            SetProperty(type, "ContentType", contentType);
            SetProperty(type, "Category", "image");
            SetProperty(type, "IsActive", isActive);
            return type;
        }

        private static void SetProperty(object target, string name, object value) =>
            target.GetType().GetProperty(name)!.SetValue(target, value);
    }
}
