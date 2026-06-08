export interface TextNoteDto {
  id: string
  userId: string
  title: string
  text: string
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface CheckListSummaryDto {
  id: string
  userId: string
  title: string
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface CheckListItemDto {
  id: string
  text: string
  isChecked: boolean
  order: number
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface CheckListDto {
  id: string
  userId: string
  title: string
  items: CheckListItemDto[]
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface CreateTextNoteBody {
  title: string
  text: string
}

export interface UpdateTextNoteBody {
  title: string
  text: string
}

export interface CreateCheckListBody {
  title: string
}

export interface UpdateCheckListTitleBody {
  title: string
}

export interface AddCheckListItemBody {
  text: string
}

export interface UpdateCheckListItemBody {
  text: string
}

export interface ReorderCheckListItemBody {
  newOrder: number
}
