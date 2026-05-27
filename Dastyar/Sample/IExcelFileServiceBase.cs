/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Common.Interfaces;

/// <summary>
/// سرویس فایل اکسل
/// </summary>
public partial interface IExcelFileService
{
    /// <summary>
    /// ساخت فایل اکسل با یک شیت
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="data">داده‌ها</param>
    /// <param name="sheetName">نام شیت</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1>(IReadOnlyList<T1> data, string sheetName) where T1 : class;

    /// <summary>
    /// ساخت فایل اکسل با تعداد شیت نامحدود و مدل مشترک
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="sheets">اطلاعات شیت‌ها</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1>(List<(IReadOnlyList<T1> data, string sheetName)> sheets) where T1 : class;

    /// <summary>
    /// ساخت فایل اکسل با دو شیت
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="sheet1">شیت اول</param>
    /// <param name="sheet2">شیت دوم</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1, T2>((IReadOnlyList<T1> Data, string SheetName) sheet1, (IReadOnlyList<T2> Data, string SheetName) sheet2)
        where T1 : class
        where T2 : class;

    /// <summary>
    /// ساخت فایل اکسل با سه شیت
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="sheet1">شیت اول</param>
    /// <param name="sheet2">شیت دوم</param>
    /// <param name="sheet3">شیت سوم</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1, T2, T3>((IReadOnlyList<T1> Data, string SheetName) sheet1, (IReadOnlyList<T2> Data, string SheetName) sheet2, (IReadOnlyList<T3> Data, string SheetName) sheet3)
        where T1 : class
        where T2 : class
        where T3 : class;

    /// <summary>
    /// ساخت فایل اکسل با چهار شیت
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="sheet1">شیت اول</param>
    /// <param name="sheet2">شیت دوم</param>
    /// <param name="sheet3">شیت سوم</param>
    /// <param name="sheet4">شیت چهارم</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1, T2, T3, T4>((IReadOnlyList<T1> Data, string SheetName) sheet1, (IReadOnlyList<T2> Data, string SheetName) sheet2, (IReadOnlyList<T3> Data, string SheetName) sheet3, (IReadOnlyList<T4> Data, string SheetName) sheet4)
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class;

    /// <summary>
    /// ساخت فایل اکسل با پنج شیت
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="sheet1">شیت اول</param>
    /// <param name="sheet2">شیت دوم</param>
    /// <param name="sheet3">شیت سوم</param>
    /// <param name="sheet4">شیت چهارم</param>
    /// <param name="sheet5">شیت پنجم</param>
    /// <returns></returns>
    byte[] ExportToExcel<T1, T2, T3, T4, T5>((IReadOnlyList<T1> Data, string SheetName) sheet1, (IReadOnlyList<T2> Data, string SheetName) sheet2, (IReadOnlyList<T3> Data, string SheetName) sheet3, (IReadOnlyList<T4> Data, string SheetName) sheet4, (IReadOnlyList<T5> Data, string SheetName) sheet5)
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class;

    /// <summary>
    /// پیش پردازش هدر ستون‌های فایل اکسل
    /// </summary>
    /// <param name="fileBytes">فایل اکسل ورودی</param>
    /// <param name="headerCellProcessor">پردازشگر هدر - مقدار اصلی هدر به عنوان ورودی داده می‌شود و مقدار پردازش شده را باید برگرداند</param>
    /// <param name="headerMappings">مپینگ نام هدرها - با استفاده از این دیکشنری نام هدرها پس از اجرای پردازشگر مپ می‌شود</param>
    /// <param name="cancellationToken"></param>
    /// <returns>قایل اکسل با هدرهای پردازش شده</returns>
    Task<byte[]> PreprocessExcelHeaderAsync(byte[] fileBytes, Func<string, string> headerCellProcessor, Dictionary<string, string> headerMappings = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// خواندن فایل
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    List<T> ReadFile<T>(Stream stream) where T : class;

    /// <summary>
    /// خواندن فایل
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <returns></returns>
    List<T> ReadFile<T>(byte[] bytes) where T : class;
}
