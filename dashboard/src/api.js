import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:5239'
const client = axios.create({ baseURL: API_BASE })

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('dastyar_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('dastyar_token')
      window.dispatchEvent(new Event('dastyar:unauthorized'))
    }
    return Promise.reject(error)
  },
)

export function setAuthToken(token) {
  if (token) {
    localStorage.setItem('dastyar_token', token)
  } else {
    localStorage.removeItem('dastyar_token')
  }
}

export function getAuthToken() {
  return localStorage.getItem('dastyar_token')
}

export async function login(payload) {
  const res = await client.post('/api/auth/login', payload)
  return res.data
}

export async function register(payload) {
  const res = await client.post('/api/auth/register', payload)
  return res.data
}

export async function fetchProducts() {
  const res = await client.get('/api/products')
  return res.data
}

export async function searchProducts(query) {
  const searchTerm = query.title || '';
  const res = await client.get('/api/products/search/simple', {
    params: { q: searchTerm }
  })
  return res.data
}

export async function createProduct(payload) {
  const res = await client.post('/api/products', payload)
  return res.data
}

export async function updateProduct(productId, payload) {
  const res = await client.put(`/api/products/${productId}`, payload)
  return res.data
}

export async function importProducts(file) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post('/api/products/import', form, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return res.data
}

export async function exportProducts(fields) {
  const res = await client.post('/api/products/export', { fields }, { responseType: 'blob' })
  const disposition = res.headers?.['content-disposition'] || ''
  const match = disposition.match(/filename="?([^";]+)"?/i)
  const fileName = match?.[1] || 'products_export.xlsx'
  return { blob: res.data, fileName }
}

export async function fetchMaterials() {
  const res = await client.get('/api/materials')
  return res.data
}

export async function searchMaterials(query) {
  const searchTerm = query.title || '';
  const res = await client.get('/api/materials/search/simple', {
    params: { q: searchTerm }
  })
  return res.data
}

export async function createMaterial(payload) {
  const res = await client.post('/api/materials', payload)
  return res.data
}

export async function updateMaterial(materialId, payload) {
  const res = await client.put(`/api/materials/${materialId}`, payload)
  return res.data
}

export async function importMaterials(file) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post('/api/materials/import', form, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return res.data
}

export async function exportMaterials(fields) {
  const res = await client.post('/api/materials/export', { fields }, { responseType: 'blob' })
  const disposition = res.headers?.['content-disposition'] || ''
  const match = disposition.match(/filename="?([^";]+)"?/i)
  const fileName = match?.[1] || 'materials_export.xlsx'
  return { blob: res.data, fileName }
}

export async function importNewCategoryMaterials(categoryId, file) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post(`/api/material-categories/${categoryId}/materials/import-new`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

export async function fetchCategories() {
  const res = await client.get('/api/categories')
  return res.data
}

export async function createCategory(payload) {
  const res = await client.post('/api/categories', payload)
  return res.data
}

export async function updateCategory(id, payload) {
  const res = await client.put(`/api/categories/${id}`, payload)
  return res.data
}

export async function fetchMaterialAddonAssignments() {
  const res = await client.get('/api/material-addon-assignments')
  return res.data
}

export async function createMaterialAddonAssignment(payload) {
  const res = await client.post('/api/material-addon-assignments', payload)
  return res.data
}

export async function updateMaterialAddonAssignment(id, payload) {
  const res = await client.put(`/api/material-addon-assignments/${id}`, payload)
  return res.data
}

export async function deleteMaterialAddonAssignment(id) {
  const res = await client.delete(`/api/material-addon-assignments/${id}`)
  return res.data
}

export async function fetchFieldDefinitions() {
  const res = await client.get('/api/field-definitions');
  return res.data;
}

export async function createFieldDefinition(payload) {
  const res = await client.post('/api/field-definitions', payload);
  return res.data;
}

export async function updateFieldDefinition(id, payload) {
  const res = await client.put(`/api/field-definitions/${id}`, payload);
  return res.data;
}

export async function deleteFieldDefinition(id) {
  const res = await client.delete(`/api/field-definitions/${id}`);
  return res.data;
}

export async function fetchMaterialCategoryTree() {
  const res = await client.get('/api/material-categories/tree')
  return res.data
}

export async function importMaterialPrices(file, categoryId = null) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post('/api/materials/import-last-price-update-only', form, {
    params: categoryId ? { categoryId } : undefined,
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

export async function updateMaterialPrices(materialId, payload) {
  const res = await client.patch(`/api/materials/${materialId}/prices`, payload)
  return res.data
}

export async function updateMaterialsDailyPricePercent(payload) {
  const res = await client.post('/api/materials/daily-price/percent', payload)
  return res.data
}

export async function fetchMaterialUsageProducts(materialId) {
  const res = await client.get(`/api/materials/${materialId}/usage-products`)
  return res.data
}

export async function fetchMaterialIdentity(materialId, priceBasis = 'last') {
  const res = await client.get('/api/reports/material-identity', {
    params: { materialId, priceBasis },
  })
  return res.data
}

export async function fetchProductIdentity(productId, priceBasis = 'last') {
  const res = await client.get('/api/reports/product-identity', {
    params: { productId, priceBasis },
  })
  return res.data
}

export default client
