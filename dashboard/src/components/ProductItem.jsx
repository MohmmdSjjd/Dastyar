import React, { useMemo, useState } from "react";
import { Button, Descriptions, Modal, Space, Tag, Typography } from "antd";
import { EditOutlined, EyeOutlined } from "@ant-design/icons";

const { Text } = Typography;

function formatPrice(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "-";
}

function AddonChips({ addons = [] }) {
  if (!addons.length) {
    return <Text type="secondary">افزودنی ندارد</Text>;
  }

  return (
    <div className="addon-chip-list">
      {addons.map((addon) => (
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
    </div>
  );
}

export default function ProductItem({ product, onEdit, layout = "list" }) {
  const [detailsVisible, setDetailsVisible] = useState(false);
  const appliedAddons = product.appliedAddons || [];

  const dynamicFields = useMemo(() => {
    if (!product.dynamicFieldsJson) return [];

    try {
      const data = JSON.parse(product.dynamicFieldsJson);
      const standardFields = [
        "code",
        "name",
        "isactive",
        "categorycode",
        "baseprice",
        "unit",
      ];

      return Object.entries(data).filter(
        ([key]) => !standardFields.includes(key.toLowerCase()),
      );
    } catch (error) {
      console.error("Error parsing dynamicFieldsJson:", error);
      return [];
    }
  }, [product.dynamicFieldsJson]);

  const detailsModal = (
    <Modal
      title={`جزئیات ماده اولیه: ${product.name || "-"}`}
      open={detailsVisible}
      onCancel={() => setDetailsVisible(false)}
      footer={[
        <Button key="close" onClick={() => setDetailsVisible(false)}>
          بستن
        </Button>,
        <Button
          key="edit"
          type="primary"
          icon={<EditOutlined />}
          onClick={() => {
            setDetailsVisible(false);
            onEdit(product);
          }}
        >
          ویرایش
        </Button>,
      ]}
    >
      <Descriptions column={1} size="small" bordered>
        <Descriptions.Item label="کد ماده اولیه">
          <code>{product.code}</code>
        </Descriptions.Item>
        <Descriptions.Item label="نام ماده اولیه">
          {product.name || "-"}
        </Descriptions.Item>
        <Descriptions.Item label="دسته‌بندی">
          {product.categoryName || "-"}
        </Descriptions.Item>
        <Descriptions.Item label="واحد">
          {product.unitEffective || "-"}
        </Descriptions.Item>
        <Descriptions.Item label="قیمت پایه">
          {formatPrice(product.basePrice)}
        </Descriptions.Item>
        <Descriptions.Item label="افزودنی‌ها">
          <AddonChips addons={appliedAddons} />
        </Descriptions.Item>
        <Descriptions.Item label="جمع افزودنی‌ها">
          {formatPrice(product.addonTotalPrice)}
        </Descriptions.Item>
        <Descriptions.Item label="قیمت نهایی">
          {formatPrice(product.finalPrice)}
        </Descriptions.Item>
        <Descriptions.Item label="وضعیت">
          <Tag color={product.isActive ? "green" : "red"}>
            {product.isActive ? "فعال" : "غیرفعال"}
          </Tag>
        </Descriptions.Item>
        <Descriptions.Item label="تاریخ ایجاد">
          {product.createdAtUtc
            ? new Date(product.createdAtUtc).toLocaleDateString("fa-IR")
            : "-"}
        </Descriptions.Item>
      </Descriptions>

      {dynamicFields.length > 0 && (
        <Descriptions
          column={1}
          size="small"
          bordered
          style={{ marginTop: 18 }}
        >
          {dynamicFields.map(([key, value]) => (
            <Descriptions.Item key={key} label={key}>
              {value !== null && value !== undefined ? String(value) : "-"}
            </Descriptions.Item>
          ))}
        </Descriptions>
      )}
    </Modal>
  );

  return (
    <>
      <Space size="small" wrap={layout !== "table"}>
        <Button
          type="primary"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => setDetailsVisible(true)}
        >
          جزئیات
        </Button>
        <Button
          type="default"
          size="small"
          icon={<EditOutlined />}
          onClick={() => onEdit(product)}
        >
          ویرایش
        </Button>
      </Space>
      {detailsModal}
    </>
  );
}
