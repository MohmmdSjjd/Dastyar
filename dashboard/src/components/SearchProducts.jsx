import React, { useState, useEffect, useRef } from "react";
import { searchMaterials } from "../api";
import { Card, Input, Button, List, message, Typography, Badge } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import ProductItem from "./ProductItem";
import EditProductModal from "./EditProductModal";

const { Text } = Typography;

export default function SearchProducts() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [editModalVisible, setEditModalVisible] = useState(false);
  const debounceTimer = useRef(null);

  const performSearch = async (searchQuery) => {
    if (!searchQuery.trim()) {
      setResults([]);
      setSearched(false);
      return;
    }

    setLoading(true);
    setSearched(true);
    try {
      const data = await searchMaterials({ title: searchQuery });
      setResults(data);
    } catch {
      message.error("خطا در جستجو");
      setResults([]);
    } finally {
      setLoading(false);
    }
  };

  const handleQueryChange = (e) => {
    const newQuery = e.target.value;
    setQuery(newQuery);

    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current);
    }

    debounceTimer.current = setTimeout(() => {
      performSearch(newQuery);
    }, 2000);
  };

  const handleInstantSearch = () => {
    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current);
    }
    performSearch(query);
  };

  const handleEditProduct = (product) => {
    setSelectedProduct(product);
    setEditModalVisible(true);
  };

  const handleEditSuccess = async () => {
    // بازبارگزاری نتایج جستجو
    if (query) {
      performSearch(query);
    }
  };

  useEffect(() => {
    return () => {
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current);
      }
    };
  }, []);

  return (
    <>
      <Card
        title={
          <div className="flex items-center gap-2">
            <span>جستجوی مواد اولیه</span>
            {searched && results.length > 0 && (
              <Badge
                count={results.length}
                style={{ backgroundColor: "#52c41a" }}
              />
            )}
          </div>
        }
        className="card"
        bordered={false}
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            handleInstantSearch();
          }}
          className="flex flex-col sm:flex-row sm:items-center gap-3 mb-4"
        >
          <div className="flex-1 relative">
            <Input
              value={query}
              onChange={handleQueryChange}
              placeholder="نام یا کد ماده اولیه را وارد کنید (جستجو خودکار بعد از 2 ثانیه)"
              className="flex-1"
              allowClear
            />
            {query && !searched && (
              <Text
                type="secondary"
                className="text-xs absolute right-3 top-1/2 transform -translate-y-1/2"
              >
                در حال انتظار...
              </Text>
            )}
          </div>
          <Button
            type="primary"
            icon={<SearchOutlined />}
            onClick={handleInstantSearch}
            loading={loading}
          >
            جستجو فوری
          </Button>
        </form>

        {searched && (
          <>
            {results.length > 0 && (
              <div className="mb-3 text-sm text-gray-600">
                <strong>{results.length}</strong> نتیجه یافت شد
              </div>
            )}
            <List
              bordered
              dataSource={results}
              loading={loading}
              locale={{
                emptyText: (
                  <Text type="secondary">
                    {loading ? "در حال جستجو..." : "نتیجه‌ای یافت نشد"}
                  </Text>
                ),
              }}
              renderItem={(item) => (
                <List.Item>
                  <ProductItem
                    product={item}
                    onEdit={handleEditProduct}
                    onRefresh={() => handleEditSuccess()}
                    layout="list"
                  />
                </List.Item>
              )}
            />
          </>
        )}
      </Card>

      {selectedProduct && (
        <EditProductModal
          product={selectedProduct}
          visible={editModalVisible}
          onClose={() => {
            setEditModalVisible(false);
            setSelectedProduct(null);
          }}
          onSuccess={handleEditSuccess}
        />
      )}
    </>
  );
}
