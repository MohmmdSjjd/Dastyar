const fs = require('fs');
const path = require('path');

// Install xlsx if needed and create Excel file
const XLSX = require('xlsx');

const workbook = XLSX.utils.book_new();

// Create worksheet with headers
const headers = ['Code', 'Name', 'IsActive', 'CategoryCode', 'BasePrice', 'Unit'];
const data = [
    headers,
    ['MAT001', 'ماده نمونه A', 'true', 'CAT001', '20000', 'متر'],
    ['MAT002', 'ماده نمونه B', 'false', 'CAT002', '15000', 'سانتیمتر'],
    ['MAT003', 'ماده نمونه C', '1', 'CAT001', '18000', 'عدد'],
    ['MAT004', 'ماده نمونه D', '0', '', '12000', 'متر'],
    ['MAT005', 'ماده نمونه E', 'yes', 'CAT003', '25000', 'کیلوگرم'],
    ['MAT006', 'ماده نمونه F', 'true', '', '10000', 'عدد'],
];

const worksheet = XLSX.utils.aoa_to_sheet(data);
XLSX.utils.book_append_sheet(workbook, worksheet, 'Materials');

const outputPath = path.join(__dirname, 'dashboard', 'sample-import-materials.xlsx');
const outputDir = path.dirname(outputPath);

// Ensure directory exists
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

XLSX.writeFile(workbook, outputPath);
console.log(`✓ Excel file created: ${outputPath}`);
