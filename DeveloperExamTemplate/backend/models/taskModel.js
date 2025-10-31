import { pool } from './db.js';

// ดึงรายการงานทั้งหมดจากฐานข้อมูลเพื่อใช้แสดงใน Front-end
export const getAllTasks = async () => {
  const result = await pool.query('SELECT id, title, description, status FROM tasks ORDER BY id');
  return result.rows;
};

// เพิ่มงานใหม่และคืนค่าข้อมูลที่ถูกบันทึกจริงในฐานข้อมูล
export const createTask = async ({ title, description }) => {
  const result = await pool.query(
    'INSERT INTO tasks (title, description) VALUES ($1, $2) RETURNING id, title, description, status',
    [title, description]
  );
  return result.rows[0];
};
