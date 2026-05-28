using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/price-formula")]
public sealed class PriceFormulaController : ControllerBase
{
    [HttpGet("menu")]
    public ActionResult<IReadOnlyList<PriceFormulaMenuDto>> GetMenu()
    {
        return Ok(new[]
        {
            new PriceFormulaMenuDto("products", "لیست محصولات", "#0ea5e9", "تعریف و مدیریت محصولات"),
            new PriceFormulaMenuDto("materials", "لیست مواد اولیه", "#10b981", "کنترل قیمت مواد و نمایش کل/جز"),
            new PriceFormulaMenuDto("product-categories", "دسته‌بندی محصولات", "#f59e0b", "مدیریت گروه‌های محصولات"),
            new PriceFormulaMenuDto("material-categories", "دسته‌بندی مواد اولیه", "#22c55e", "مدیریت گروه‌های مواد و ثبت جدید"),
            new PriceFormulaMenuDto("reports", "گزارشات", "#3b82f6", "شناسنامه و قیمت تمام‌شده"),
        });
    }
}
