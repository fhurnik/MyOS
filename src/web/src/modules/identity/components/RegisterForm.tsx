"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import Link from "next/link"
import { useTranslations } from "next-intl"
import { registerSchema, type RegisterFormValues } from "@/modules/identity/schemas/register.schema"
import { useRegister } from "@/modules/identity/hooks/useRegister"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { Alert } from "@/shared/components/ui/alert"

export function RegisterForm() {
  const t = useTranslations("identity.register")
  const { mutate: register, isPending, error } = useRegister()

  const {
    register: field,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = (values: RegisterFormValues) => register(values)

  const errorMessage =
    error instanceof Error ? error.message : error ? String(error) : null

  return (
    <div className="w-full max-w-sm space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        {errorMessage && (
          <Alert>
            <p className="text-sm">{errorMessage}</p>
          </Alert>
        )}

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="firstName">{t("firstName")}</Label>
            <Input
              id="firstName"
              aria-invalid={!!errors.firstName}
              {...field("firstName")}
            />
            {errors.firstName && (
              <p className="text-sm text-destructive">{errors.firstName.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="lastName">{t("lastName")}</Label>
            <Input
              id="lastName"
              aria-invalid={!!errors.lastName}
              {...field("lastName")}
            />
            {errors.lastName && (
              <p className="text-sm text-destructive">{errors.lastName.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="email">{t("email")}</Label>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            aria-invalid={!!errors.email}
            {...field("email")}
          />
          {errors.email && (
            <p className="text-sm text-destructive">{errors.email.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password">{t("password")}</Label>
          <Input
            id="password"
            type="password"
            autoComplete="new-password"
            aria-invalid={!!errors.password}
            {...field("password")}
          />
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password.message}</p>
          )}
        </div>

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending ? "..." : t("submit")}
        </Button>
      </form>

      <p className="text-center text-sm text-muted-foreground">
        {t("hasAccount")}{" "}
        <Link href="../login" className="text-primary underline-offset-4 hover:underline">
          {t("loginLink")}
        </Link>
      </p>
    </div>
  )
}
