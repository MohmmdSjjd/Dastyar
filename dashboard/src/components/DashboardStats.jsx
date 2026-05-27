import React, { useEffect, useMemo, useState } from "react";
import { Card, Col, Empty, Row, Skeleton, Statistic, Typography } from "antd";
import {
  fetchCategories,
  fetchMaterialAddonAssignments,
  fetchMaterials,
} from "../api";

const { Text, Title } = Typography;

function formatNumber(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "۰";
}

function formatPrice(value) {
  return `${formatNumber(value || 0)} تومان`;
}

function percent(value, total) {
  if (!total) return 0;
  return Math.round((value / total) * 100);
}

function HorizontalBars({ items, valueKey = "value", labelKey = "label", suffix = "" }) {
  const max = Math.max(...items.map((item) => item[valueKey] || 0), 1);

  if (!items.length) {
    return <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="داده‌ای برای نمایش نیست" />;
  }

  return (
    <div className="chart-bars">
      {items.map((item) => {
        const value = item[valueKey] || 0;
        return (
          <div className="chart-bar-row" key={item[labelKey]}>
            <div className="chart-bar-label">
              <strong>{item[labelKey]}</strong>
              <span>{formatNumber(value)}{suffix}</span>
            </div>
            <div className="chart-bar-track">
              <div
                className="chart-bar-fill"
                style={{ width: `${Math.max(7, (value / max) * 100)}%` }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}

function PriceStack({ baseTotal, addonTotal }) {
  const finalTotal = baseTotal + addonTotal;
  const basePercent = percent(baseTotal, finalTotal);
  const addonPercent = percent(addonTotal, finalTotal);

  return (
    <div className="price-stack">
      <div className="price-stack-line">
        <div className="price-stack-base" style={{ width: `${basePercent}%` }} />
        <div className="price-stack-addon" style={{ width: `${addonPercent}%` }} />
      </div>
      <div className="price-stack-legend">
        <span>
          <i className="legend-dot base" /> قیمت پایه: {formatPrice(baseTotal)}
        </span>
        <span>
          <i className="legend-dot addon" /> افزودنی‌ها: {formatPrice(addonTotal)}
        </span>
      </div>
    </div>
  );
}

function Donut({ active, inactive }) {
  const total = active + inactive;
  const activePercent = percent(active, total);

  return (
    <div className="dashboard-donut-wrap">
      <div
        className="dashboard-donut"
        style={{
          background: `conic-gradient(#059669 0 ${activePercent}%, #dbe7e1 ${activePercent}% 100%)`,
        }}
      >
        <div>
          <strong>{formatNumber(activePercent)}٪</strong>
          <span>فعال</span>
        </div>
      </div>
      <div className="donut-legend">
        <span>
          <i className="legend-dot base" /> فعال: {formatNumber(active)}
        </span>
        <span>
          <i className="legend-dot muted" /> غیرفعال: {formatNumber(inactive)}
        </span>
      </div>
    </div>
  );
}

function AddonChips({ items }) {
  if (!items.length) {
    return <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="هنوز افزودنی روی مواد اعمال نشده است" />;
  }

  return (
    <div className="dashboard-chip-list">
      {items.map((material) => (
        <div className="dashboard-chip-card" key={material.id}>
          <div>
            <strong>{material.name || material.code}</strong>
            <Text type="secondary">{material.categoryName || "بدون دسته"}</Text>
          </div>
          <span>{formatPrice(material.addonTotalPrice)}</span>
        </div>
      ))}
    </div>
  );
}

export default function DashboardStats() {
  const [data, setData] = useState({
    materials: [],
    categories: [],
    addons: [],
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;

    Promise.all([
      fetchMaterials(),
      fetchCategories(),
      fetchMaterialAddonAssignments(),
    ])
      .then(([materials, categories, addons]) => {
        if (!mounted) return;

        setData({
          materials: Array.isArray(materials) ? materials : [],
          categories: Array.isArray(categories) ? categories : [],
          addons: Array.isArray(addons) ? addons : [],
        });
      })
      .catch(() => {
        if (mounted) {
          setData({ materials: [], categories: [], addons: [] });
        }
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, []);

  const analytics = useMemo(() => {
    const { materials, categories, addons } = data;
    const totalBase = materials.reduce((sum, item) => sum + (item.basePrice || 0), 0);
    const totalAddon = materials.reduce((sum, item) => sum + (item.addonTotalPrice || 0), 0);
    const totalFinal = materials.reduce((sum, item) => sum + (item.finalPrice || 0), 0);
    const activeCount = materials.filter((item) => item.isActive).length;
    const inactiveCount = materials.length - activeCount;
    const withAddons = materials.filter(
      (item) => (item.appliedAddons?.length || 0) > 0 || (item.addonTotalPrice || 0) > 0,
    ).length;

    const categoryMap = new Map();
    categories.forEach((category) => categoryMap.set(category.code, category.name));
    materials.forEach((material) => {
      const label =
        material.categoryName ||
        categoryMap.get(material.categoryCode) ||
        "بدون دسته";
      categoryMap.set(label, label);
    });

    const categoryCounts = materials.reduce((map, material) => {
      const label = material.categoryName || "بدون دسته";
      map.set(label, (map.get(label) || 0) + 1);
      return map;
    }, new Map());

    const categoryItems = Array.from(categoryCounts.entries())
      .map(([label, value]) => ({ label, value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 8);

    const topExpensive = [...materials]
      .sort((a, b) => (b.finalPrice || 0) - (a.finalPrice || 0))
      .slice(0, 6)
      .map((item) => ({
        label: item.name || item.code || "-",
        value: item.finalPrice || 0,
      }));

    const topAddonImpact = [...materials]
      .filter((item) => (item.addonTotalPrice || 0) > 0)
      .sort((a, b) => (b.addonTotalPrice || 0) - (a.addonTotalPrice || 0))
      .slice(0, 5);

    return {
      materialsCount: materials.length,
      categoriesCount: categories.length,
      addonRulesCount: addons.length,
      activeCount,
      inactiveCount,
      withAddons,
      averageFinal: materials.length ? Math.round(totalFinal / materials.length) : 0,
      totalBase,
      totalAddon,
      totalFinal,
      categoryItems,
      topExpensive,
      topAddonImpact,
    };
  }, [data]);

  if (loading) {
    return (
      <div className="dashboard-grid">
        {Array.from({ length: 6 }).map((_, index) => (
          <Card className="card" key={index} bordered={false}>
            <Skeleton active />
          </Card>
        ))}
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="card dashboard-stat-card">
            <Statistic title="مواد اولیه" value={analytics.materialsCount} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="card dashboard-stat-card">
            <Statistic title="دسته‌بندی‌ها" value={analytics.categoriesCount} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="card dashboard-stat-card">
            <Statistic title="قواعد افزودنی" value={analytics.addonRulesCount} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="card dashboard-stat-card">
            <Statistic title="میانگین قیمت نهایی" value={analytics.averageFinal} suffix="تومان" />
          </Card>
        </Col>
      </Row>

      <div className="dashboard-grid">
        <Card bordered={false} className="card dashboard-chart-card">
          <div className="chart-card-title">
            <Title level={5}>ترکیب قیمت کل</Title>
            <Text type="secondary">سهم قیمت پایه و افزودنی‌ها</Text>
          </div>
          <PriceStack
            baseTotal={analytics.totalBase}
            addonTotal={analytics.totalAddon}
          />
          <div className="dashboard-total-price">
            <span>جمع قیمت نهایی</span>
            <strong>{formatPrice(analytics.totalFinal)}</strong>
          </div>
        </Card>

        <Card bordered={false} className="card dashboard-chart-card">
          <div className="chart-card-title">
            <Title level={5}>وضعیت مواد</Title>
            <Text type="secondary">
              {formatNumber(analytics.withAddons)} ماده دارای افزودنی هستند
            </Text>
          </div>
          <Donut active={analytics.activeCount} inactive={analytics.inactiveCount} />
        </Card>

        <Card bordered={false} className="card dashboard-chart-card wide">
          <div className="chart-card-title">
            <Title level={5}>توزیع مواد در دسته‌بندی‌ها</Title>
            <Text type="secondary">۸ دسته‌بندی پرتعداد</Text>
          </div>
          <HorizontalBars items={analytics.categoryItems} suffix=" مورد" />
        </Card>

        <Card bordered={false} className="card dashboard-chart-card">
          <div className="chart-card-title">
            <Title level={5}>گران‌ترین مواد</Title>
            <Text type="secondary">بر اساس قیمت نهایی</Text>
          </div>
          <HorizontalBars items={analytics.topExpensive} suffix=" تومان" />
        </Card>

        <Card bordered={false} className="card dashboard-chart-card">
          <div className="chart-card-title">
            <Title level={5}>بیشترین اثر افزودنی</Title>
            <Text type="secondary">موادی که افزودنی بیشتری به قیمتشان اضافه کرده</Text>
          </div>
          <AddonChips items={analytics.topAddonImpact} />
        </Card>
      </div>
    </div>
  );
}
