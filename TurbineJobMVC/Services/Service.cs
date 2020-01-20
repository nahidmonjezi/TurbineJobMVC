﻿using Arch.EntityFrameworkCore.UnitOfWork;
using AutoMapper;
using MD.PersianDateTime.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurbineJobMVC.Extensions;
using TurbineJobMVC.Models.Entites;
using TurbineJobMVC.Models.ViewModels;

namespace TurbineJobMVC.Services
{
    public class Service : IService
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IMapper _map;
        public Service(IUnitOfWork unitofwork, IMapper map)
        {
            _unitofwork = unitofwork;
            _map = map;
        }
        public async Task<long> addWorkOrder(JobViewModel JobModel)
        {
            var repo = _unitofwork.GetRepository<WorkOrderTBL>();
            var repoWorkOrderComment = _unitofwork.GetRepository<WorkOrderDailyReportTBL>();
            var newWorkOrder = _map.Map<WorkOrderTBL>(JobModel);
            newWorkOrder.WOTime = DateTime.Now.ToString("HH:MM");
            newWorkOrder.WODate = DateExtensions.ConvertToWesternArbicNumerals(new PersianDateTime(DateTime.Now).ToShortDateString());
            newWorkOrder.RequestDate = newWorkOrder.WODate;
            var recordsExists = repo.Exists(q => q.WODate.Substring(0, 4) == newWorkOrder.WODate.Substring(0, 4));
            if (recordsExists)
                newWorkOrder.WONo = repo.GetAll(q => q.WODate.Substring(0, 4) == newWorkOrder.WODate.Substring(0, 4)).Max(q => q.WONo) + 1;
            else
                newWorkOrder.WONo = Convert.ToInt64((newWorkOrder.WODate.Substring(0, 4) + "000001"));
            newWorkOrder.ConsComment = 45;
            newWorkOrder.OprCode = null;
            newWorkOrder.WoType = 1;
            var ARInfo = _map.Map<TahvilFormsViewModel>(_unitofwork.GetRepository<TahvilForms>().GetFirstOrDefault(predicate: q => q.AmvalNo.ToString() == JobModel.AR));
            if (ARInfo != null)
            {
                newWorkOrder.AskerCode = ARInfo.DevilerCodeOrigin;
                var workOrderReport = new WorkOrderDailyReportTBL()
                {
                    Wono = newWorkOrder.WONo,
                    ReportID = Guid.NewGuid(),
                    ReportDate = DateTime.Now,
                    ReportComment = "درخواست کاربر دریافت گردید"
                };
                await repo.InsertAsync(newWorkOrder);
                await _unitofwork.SaveChangesAsync();
                await repoWorkOrderComment.InsertAsync(workOrderReport);
                await _unitofwork.SaveChangesAsync();
            }
            else
            {
                newWorkOrder.WONo = -1;
            }
            return newWorkOrder.WONo;
        }

        public async Task<WorkOrderViewModel> GetSingleWorkOrder(string Wono)
        {
            return _map.Map<WorkOrderViewModel>(await _unitofwork.GetRepository<WorkOrder>().GetFirstOrDefaultAsync(predicate: q => q.WONo == Convert.ToInt64(Wono)));
        }

        public async Task<TahvilFormsViewModel> GetTahvilForm(string amval)
        {
            return _map.Map<TahvilFormsViewModel>(await _unitofwork.GetRepository<TahvilForms>().GetFirstOrDefaultAsync(predicate: q => q.AmvalNo.ToString() == amval));
        }

        public async Task<IList<TahvilFormsViewModel>> GetTahvilForms(string regNo)
        {
            return _map.Map<IList<TahvilFormsViewModel>>(await _unitofwork.GetRepository<TahvilForms>().GetAllAsync(predicate: q => q.RegisterNo == regNo, orderBy: q => q.OrderByDescending(c => c.AmvalNo)));
        }
        public async Task<IList<WorkOrderDailyReportViewModel>> GetWorkOrderReport(string Wono)
        {
            return _map.Map<IList<WorkOrderDailyReportViewModel>>(await _unitofwork.GetRepository<WorkOrderDailyReportTBL>().GetAllAsync(predicate: q => q.Wono == Convert.ToInt64(Wono), orderBy: q => q.OrderByDescending(c => c.ReportDate)));
        }

        public bool IsNumberic(string number)
        {
            double tempInt = 0;
            return double.TryParse(number, out tempInt);
        }
    }
}
