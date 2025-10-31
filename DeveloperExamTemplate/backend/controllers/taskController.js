import Joi from 'joi';
import { createTask, getAllTasks } from '../models/taskModel.js';

// สร้างสคีมาสำหรับตรวจสอบข้อมูลที่ผู้ใช้ส่งเข้ามา
const taskSchema = Joi.object({
  title: Joi.string().min(3).max(100).required(),
  description: Joi.string().min(5).max(500).required()
});

// Controller สำหรับดึงข้อมูลรายการงานทั้งหมด
export const handleGetTasks = async (_req, res, next) => {
  try {
    const tasks = await getAllTasks();
    res.json(tasks);
  } catch (error) {
    next(error);
  }
};

// Controller สำหรับบันทึกงานใหม่ลงฐานข้อมูล
export const handleCreateTask = async (req, res, next) => {
  try {
    const { error, value } = taskSchema.validate(req.body);
    if (error) {
      // ตอบกลับด้วยสถานะ 400 เพื่อสื่อว่าข้อมูลไม่ถูกต้อง
      return res.status(400).json({ message: error.message });
    }

    const newTask = await createTask(value);
    res.status(201).json(newTask);
  } catch (error) {
    next(error);
  }
};
