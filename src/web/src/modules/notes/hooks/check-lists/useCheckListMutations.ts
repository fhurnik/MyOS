"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  createCheckListApi,
  updateCheckListTitleApi,
  deleteCheckListApi,
  addCheckListItemApi,
  updateCheckListItemApi,
  deleteCheckListItemApi,
  toggleCheckListItemApi,
  reorderCheckListItemApi,
} from "@/modules/notes/api/check-lists.api"
import { checkListKeys } from "./query-keys"

export function useCreateCheckList() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: createCheckListApi,
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.lists() }),
  })
}

export function useUpdateCheckList(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: Parameters<typeof updateCheckListTitleApi>[1]) =>
      updateCheckListTitleApi(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: checkListKeys.lists() })
      qc.invalidateQueries({ queryKey: checkListKeys.detail(id) })
    },
  })
}

export function useDeleteCheckList() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: deleteCheckListApi,
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.lists() }),
  })
}

export function useAddCheckListItem(checkListId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: Parameters<typeof addCheckListItemApi>[1]) =>
      addCheckListItemApi(checkListId, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.detail(checkListId) }),
  })
}

export function useUpdateCheckListItem(checkListId: string, itemId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: Parameters<typeof updateCheckListItemApi>[2]) =>
      updateCheckListItemApi(checkListId, itemId, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.detail(checkListId) }),
  })
}

export function useDeleteCheckListItem(checkListId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (itemId: string) => deleteCheckListItemApi(checkListId, itemId),
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.detail(checkListId) }),
  })
}

export function useToggleCheckListItem(checkListId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (itemId: string) => toggleCheckListItemApi(checkListId, itemId),
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.detail(checkListId) }),
  })
}

export function useReorderCheckListItem(checkListId: string, itemId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: Parameters<typeof reorderCheckListItemApi>[2]) =>
      reorderCheckListItemApi(checkListId, itemId, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: checkListKeys.detail(checkListId) }),
  })
}
