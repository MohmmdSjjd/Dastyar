import React, { useEffect, useMemo, useState } from "react";
import { fetchMaterials, fetchCategories, exportMaterials } from "../api";
import {
  Badge,
  Button,
  Card,
  Checkbox,
  Empty,
  Input,
  message,
  Modal,
  Space,
  Tag,
  Tree,
  Typography,
} from "antd";
import {
  FileExcelOutlined,
  ReloadOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import ProductItem from "./ProductItem";
import EditProductModal from "./EditProductModal";

const { Text, Title } = Typography;

function formatPrice(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "-";
}

function buildTree(categories) {
  const nodesById = new Map();
  const roots = [];

  categories.forEach((category) => {
    nodesById.set(category.id, {
      key: category.code,
      title: category.name,
      children: [],
    });
  });

  categories.forEach((category) => {
    const node = nodesById.get(category.id);
    if (!node) return;

    if (category.parentCategoryId && nodesById.has(category.parentCategoryId)) {
      nodesById.get(category.parentCategoryId).children.push(node);
    } else {
      roots.push(node);
    }
  });

  return [
    {
      key: "all",
      title: "همه مواد اولیه",
      children: roots,
    },
  ];
}

function AddonPreview({ addons = [] }) {
  if (!addons.length) {
    return <Text type="secondary">بدون افزودنی</Text>;
  }

  return (
    <div className="addon-chip-list compact">
      {addons.slice(0, 3).map((addon) => (
        <span className="addon-chip" key={addon.assignmentId || addon.code}>
          <strong>{addon.name || addon.code}</strong>
          <span>
            {"{"}
            {formatPrice(addon.unitPrice)} × {addon.quantity}
            {addon.unit ? ` ${addon.unit}` : ""} = {formatPrice(addon.totalPrice)}
            {"}"}
          </span>
        </span>
      ))}
      {addons.length > 3 && (
        <span className="addon-chip more">+{addons.length - 3}</span>
      )}
    </div>
  );
}

export default function ProductsList() {
  const [materials, setMaterials] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [exportModalVisible, setExportModalVisible] = useState(false);
  const [availableFields, setAvailableFields] = useState([]);
  const [selectedFields, setSelectedFields] = useState([]);
  const [selectedCategoryCode, setSelectedCategoryCode] = useState("all");
  const [query, setQuery] = useState("");

  const loadProducts = async () => {
    setLoading(true);
    try {
      const [materialsData, categoriesData] = await Promise.all([
        fetchMaterials(),
        fetchCategories(),
      ]);
      setMaterials(materialsData);
      setCategories(categoriesData);
    } catch {
      message.error("خطا در دریافت مواد اولیه");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProducts();
  }, []);

  const handleEditProduct = (product) => {
    setSelectedProduct(product);
    setEditModalVisible(true);
  };

  const treeData = useMemo(() => buildTree(categories), [categories]);

  const filteredMaterials = useMemo(() => {
    const normalized = query.trim().toLowerCase();
    return materials.filter((material) => {
      const matchesCategory =
        selectedCategoryCode === "all" ||
        material.categoryCode === selectedCategoryCode;
      const matchesSearch =
        !normalized ||
        material.name?.toLowerCase().includes(normalized) ||
        material.code?.toLowerCase().includes(normalized);

      return matchesCategory && matchesSearch;
    });
  }, [materials, query, selectedCategoryCode]);

  const selectedCategoryName =
    selectedCategoryCode === "all"
      ? "همه دسته‌ها"
      : categories.find((category) => category.code === selectedCategoryCode)?.name ||
        selectedCategoryCode;

  const openExport = () => {
    const keys = new Set();
    const standardFields = ["Code", "Name", "BasePrice", "Unit", "CategoryCode"];
    standardFields.forEach((field) => keys.add(field));

    materials.forEach((material) => {
      try {
        const data = material.dynamicFieldsJson
          ? JSON.parse(material.dynamicFieldsJson)
          : {};
        Object.keys(data || {}).forEach((key) => keys.add(key));
      } catch {
        // Ignore malformed dynamic JSON.
      }
    });

    setAvailableFields(Array.from(keys));
    setSelectedFields([]);
    setExportModalVisible(true);
  };

  return (
    <>
      <Card
        bordered={false}
        className="card materials-board"
        title={
          <div className="materials-title">
            <span>لیست مواد اولیه</span>
            <Badge count={materials.length} style={{ backgroundColor: "#059669" }} />
          </div>
        }
        extra={
          <Space wrap>
            <Button icon={<ReloadOutlined />} onClick={loadProducts} loading={loading}>
              تازه‌سازی
            </Button>
            <Button icon={<FileExcelOutlined />} onClick={openExport}>
              خروجی Excel
            </Button>
          </Space>
        }
      >
        <div className="materials-layout">
          <Card size="small" title="دسته‌بندی‌ها" bordered={false} className="filter-panel">
            <Tree
              treeData={treeData}
              defaultExpandAll
              selectedKeys={[selectedCategoryCode]}
              onSelect={(keys) => {
                setSelectedCategoryCode(keys[0] || "all");
              }}
            />
          </Card>

          <div className="materials-content">
            <div className="materials-toolbar">
              <div>
                <Text type="secondary">نمایش</Text>
                <Title level={5}>{selectedCategoryName}</Title>
              </div>
              <Input
                allowClear
                prefix={<SearchOutlined />}
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="جستجوی نام یا کد"
                className="materials-search"
              />
            </div>

            {filteredMaterials.length === 0 ? (
              <Empty description="هیچ ماده‌ای یافت نشد" />
            ) : (
              <div className="materials-grid">
                {filteredMaterials.map((material) => (
                  <div className="material-card" key={material.id}>
                    <div className="material-card-head">
                      <div>
                        <h3>{material.name || "-"}</h3>
                        <Text type="secondary">
                          کد: <code>{material.code}</code>
                        </Text>
                      </div>
                      <Tag color={material.isActive ? "green" : "default"}>
                        {material.unitEffective || "بدون واحد"}
                      </Tag>
                    </div>

                    <div className="material-prices">
                      <div>
                        <span>قیمت پایه</span>
                        <strong>{formatPrice(material.basePrice)}</strong>
                      </div>
                      <div>
                        <span>قیمت نهایی</span>
                        <strong>{formatPrice(material.finalPrice)}</strong>
                      </div>
                    </div>

                    <div className="material-addon-preview">
                      <span>افزودنی‌ها</span>
                      <AddonPreview addons={material.appliedAddons || []} />
                    </div>

                    <div className="material-card-foot">
                      <Text type="secondary">{material.categoryName || "بدون دسته"}</Text>
                      <ProductItem
                        product={material}
                        onEdit={handleEditProduct}
                        layout="card"
                      />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </Card>

      {selectedProduct && (
        <EditProductModal
          product={selectedProduct}
          visible={editModalVisible}
          onClose={() => {
            setEditModalVisible(false);
            setSelectedProduct(null);
          }}
          onSuccess={loadProducts}
        />
      )}

      <Modal
        title="خروجی اکسل مواد اولیه"
        open={exportModalVisible}
        onCancel={() => setExportModalVisible(false)}
        onOk={async () => {
          try {
            const { blob, fileName } = await exportMaterials(selectedFields);
            const url = window.URL.createObjectURL(new Blob([blob]));
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = fileName || "materials_export.xlsx";
            document.body.appendChild(anchor);
            anchor.click();
            anchor.remove();
            window.URL.revokeObjectURL(url);
            setExportModalVisible(false);
          } catch {
            message.error("خطا در خروجی‌گیری");
          }
        }}
        okText="دریافت فایل"
        cancelText="لغو"
      >
        <Checkbox.Group
          className="export-fields-grid"
          options={availableFields}
          value={selectedFields}
          onChange={setSelectedFields}
        />
      </Modal>
    </>
  );
}
