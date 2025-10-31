import { Router } from 'express';
import { handleCreateTask, handleGetTasks } from '../controllers/taskController.js';

const router = Router();

// GET /api/tasks - ดึงรายการงานทั้งหมดจากฐานข้อมูล
router.get('/', handleGetTasks);

// POST /api/tasks - เพิ่มงานใหม่ตามข้อมูลที่ส่งมาจาก Front-end
router.post('/', handleCreateTask);

export default router;
