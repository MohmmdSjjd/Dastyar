from openpyxl import Workbook
from pathlib import Path

wb = Workbook()
ws = wb.active
ws.title = 'Materials'

headers = ['Code', 'Name', 'IsActive', 'CategoryCode', 'BasePrice', 'Unit']
ws.append(headers)

rows = [
    ['MAT001', 'ماده نمونه A', True, 'CAT001', 20000, 'متر'],
    ['MAT002', 'ماده نمونه B', False, 'CAT002', 15000, 'سانتیمتر'],
    ['MAT003', 'ماده نمونه C', 1, 'CAT001', 18000, 'عدد'],
    ['MAT004', 'ماده نمونه D', 0, '', 12000, 'متر'],
    ['MAT005', 'ماده نمونه E', 'yes', 'CAT003', 25000, 'کیلوگرم'],
]
for row in rows:
    ws.append(row)

output = Path('dashboard/sample-import-materials.xlsx')
output.parent.mkdir(parents=True, exist_ok=True)
wb.save(output)
print(output.resolve())
