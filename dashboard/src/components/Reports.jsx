import React, { useEffect, useMemo, useState } from "react";
import { Card, Select, Button, InputNumber, Space, Table, Typography, message, Divider, Radio } from "antd";
import { fetchMaterials, fetchProducts, fetchMaterialIdentity, fetchProductIdentity } from "../api";

const { Text } = Typography;

function formatPrice(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "-";
}

export default function Reports() {
  const [materials, setMaterials] = useState([]);
  const [products, setProducts] = useState([]);
  const [mode, setMode] = useState("material");
  const [selectedMaterialId, setSelectedMaterialId] = useState(null);
  const [selectedProductId, setSelectedProductId] = useState(null);
  const [priceBasis, setPriceBasis] = useState("last");
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    Promise.all([fetchMaterials(), fetchProducts()])
      .then(([materialsData, productsData]) => {
        setMaterials(Array.isArray(materialsData) ? materialsData : []);
        setProducts(Array.isArray(productsData) ? productsData : []);
      })
      .catch(() => message.error("خطا در دریافت اطلاعات گزارش"));
  }, []);

  const materialOptions = useMemo(
    () => materials.map((item) => ({ label: `${item.name || "-"} (${item.code || "-"})`, value: item.id })),
    [materials],
  );

  const productOptions = useMemo(
    () => products.map((item) => ({ label: `${item.name || "-"} (${item.code || "-"})`, value: item.id })),
    [products],
  );

  const runReport = async () => {
    if (mode === "material" && !selectedMaterialId) {
      message.warning("یک ماده اولیه انتخاب کنید");
      return;
    }
    if (mode === "product" && !selectedProductId) {
      message.warning("یک محصول انتخاب کنید");
      return;
    }

    setLoading(true);
    try {
      const data =
        mode === "material"
          ? await fetchMaterialIdentity(selectedMaterialId, priceBasis)
          : await fetchProductIdentity(selectedProductId, priceBasis);
      setReport(data);
    } catch (error) {
      message.error(error.response?.data?.error || "خطا در تولید گزارش");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="گزارشات و شناسنامه" bordered={false} className="card">
      <Space direction="vertical" className="w-full" size="middle">
        <Radio.Group value={mode} onChange={(e) => setMode(e.target.value)}>
          <Radio.Button value="material">شناسنامه مواد اولیه</Radio.Button>
          <Radio.Button value="product">شناسنامه محصولات</Radio.Button>
        </Radio.Group>

        <Space wrap>
          {mode === "material" ? (
            <Select
              showSearch
              optionFilterProp="label"
              placeholder="انتخاب ماده اولیه"
              options={materialOptions}
              value={selectedMaterialId}
              onChange={setSelectedMaterialId}
              style={{ minWidth: 280 }}
            />
          ) : (
            <Select
              showSearch
              optionFilterProp="label"
              placeholder="انتخاب محصول"
              options={productOptions}
              value={selectedProductId}
              onChange={setSelectedProductId}
              style={{ minWidth: 280 }}
            />
          )}

          <Select
            value={priceBasis}
            onChange={setPriceBasis}
            options={[
              { label: "فی آخرین خرید", value: "last" },
              { label: "فی خرید روز", value: "daily" },
            ]}
            style={{ minWidth: 220 }}
          />

          <Button type="primary" loading={loading} onClick={runReport}>
            دریافت گزارش
          </Button>
        </Space>

        {report && mode === "material" && (
          <>
            <Divider orientation="right">نتیجه شناسنامه مواد اولیه</Divider>
            <Space direction="vertical" className="w-full">
              <Text>کد: <code>{report.materialCode}</code></Text>
              <Text>نام: {report.materialName}</Text>
              <Text>دسته: {report.categoryName || "-"}</Text>
              <Text>تعداد محصولات مصرف‌کننده: {report.usageProductCount}</Text>
              <Text>فی انتخاب‌شده: {formatPrice(report.selectedUnitPrice)}</Text>
            </Space>
            <Table
              style={{ marginTop: 16 }}
              dataSource={report.usedInProducts || []}
              rowKey={(item) => item.productId}
              pagination={false}
              columns={[
                { title: 'کد محصول', dataIndex: 'productCode', key: 'productCode' },
                { title: 'نام محصول', dataIndex: 'productName', key: 'productName' },
                { title: 'مقدار', dataIndex: 'quantity', key: 'quantity' },
                { title: 'جمع', dataIndex: 'lineTotal', key: 'lineTotal', render: (value) => formatPrice(value) },
              ]}
            />
          </>
        )}

        {report && mode === "product" && (
          <>
            <Divider orientation="right">نتیجه شناسنامه محصولات</Divider>
            <Space direction="vertical" className="w-full">
              <Text>کد: <code>{report.productCode}</code></Text>
              <Text>نام: {report.productName}</Text>
              <Text>دسته: {report.categoryName || "-"}</Text>
              <Text>قیمت تمام‌شده: {formatPrice(report.totalCost)}</Text>
            </Space>
            <Table
              style={{ marginTop: 16 }}
              dataSource={report.materials || []}
              rowKey={(item) => item.id}
              pagination={false}
              columns={[
                { title: 'کد ماده', dataIndex: 'materialCode', key: 'materialCode' },
                { title: 'نام ماده', dataIndex: 'materialName', key: 'materialName' },
                { title: 'مقدار', dataIndex: 'quantity', key: 'quantity' },
                { title: 'فی', dataIndex: 'unitPrice', key: 'unitPrice', render: (value) => formatPrice(value) },
                { title: 'جمع', dataIndex: 'lineTotal', key: 'lineTotal', render: (value) => formatPrice(value) },
              ]}
            />
          </>
        )}
      </Space>
    </Card>
  );
}
