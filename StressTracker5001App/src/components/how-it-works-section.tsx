import {
  ArrowRight,
  BarChart3,
  CheckCircle,
  LayoutDashboard,
  ListChecks,
  Sparkles,
  Users,
} from "lucide-react";

export function HowItWorksSection() {
  return (
    <section
      id="how-it-works"
      className="py-20 sm:py-28 bg-muted/30 relative overflow-hidden"
    >
      {/* Animated background pattern */}
      <div className="absolute inset-0">
        <div className="absolute top-10 left-1/4 w-64 h-64 bg-foreground/5 rounded-full blur-3xl animate-float" />
        <div className="absolute bottom-20 right-1/3 w-80 h-80 bg-foreground/5 rounded-full blur-3xl animate-float delay-300" />
        <div className="absolute top-1/2 left-10 w-40 h-40 bg-foreground/5 rounded-full blur-2xl animate-float delay-600" />
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center mb-20">
          <div className="inline-flex items-center gap-2 bg-foreground/5 backdrop-blur-sm text-foreground px-4 py-2 rounded-full text-sm font-medium mb-6 animate-fade-up">
            <Sparkles className="h-4 w-4" />
            <span>Simple 3-step process</span>
          </div>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-foreground mb-4 text-balance animate-fade-up delay-100">
            Get Started in Minutes
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto text-pretty leading-relaxed animate-fade-up delay-200">
            Simple setup process to start managing your tasks effectively
          </p>
        </div>

        <div className="max-w-6xl mx-auto">
          <div className="relative grid md:grid-cols-3 gap-8 lg:gap-12">
            {[
              {
                number: "1",
                icon: LayoutDashboard,
                title: "Create Your Board",
                description:
                  "Set up your first Kanban board with custom columns that match your workflow",
                delay: "delay-300",
              },
              {
                number: "2",
                icon: ListChecks,
                title: "Add Your Tasks",
                description:
                  "Create tasks with details, priorities, and due dates to organize your work",
                delay: "delay-400",
              },
              {
                number: "3",
                icon: BarChart3,
                title: "Track Progress",
                description:
                  "Move tasks across columns and watch your productivity soar with clear visualization",
                delay: "delay-500",
              },
            ].map((step, index) => (
              <div
                key={index}
                className={`relative group animate-scale-in ${step.delay}`}
              >
                {/* Decorative corner elements */}
                <div className="absolute -top-2 -left-2 w-4 h-4 border-t-2 border-l-2 border-foreground/20 group-hover:border-foreground/40 transition-colors duration-300" />
                <div className="absolute -bottom-2 -right-2 w-4 h-4 border-b-2 border-r-2 border-foreground/20 group-hover:border-foreground/40 transition-colors duration-300" />

                <div className="relative bg-background/50 backdrop-blur-sm rounded-2xl p-8 hover:bg-background/80 transition-all duration-500 hover:shadow-2xl hover:-translate-y-2 border border-border hover:border-foreground/20">
                  {/* Floating number badge */}
                  <div className="absolute -top-4 left-8 h-12 w-12 rounded-full bg-foreground text-background flex items-center justify-center text-xl font-bold shadow-lg group-hover:scale-125 group-hover:rotate-12 transition-all duration-500 z-10">
                    {step.number}
                  </div>

                  {/* Icon with animation */}
                  <div className="mt-6 mb-6">
                    <div className="h-20 w-20 rounded-2xl bg-foreground/5 flex items-center justify-center mx-auto group-hover:bg-foreground/10 group-hover:scale-110 group-hover:rotate-6 transition-all duration-500 relative">
                      <step.icon className="h-10 w-10 text-foreground" />
                      {/* Animated ring around icon */}
                      <div className="absolute inset-0 rounded-2xl border-2 border-foreground/20 group-hover:scale-125 group-hover:opacity-0 transition-all duration-500" />
                    </div>
                  </div>

                  <h3 className="text-xl font-bold text-foreground mb-3 text-center">
                    {step.title}
                  </h3>
                  <p className="text-muted-foreground leading-relaxed text-center">
                    {step.description}
                  </p>

                  {/* Animated bottom accent */}
                  <div className="absolute bottom-0 left-0 right-0 h-1 bg-linear-to-r from-transparent via-foreground/30 to-transparent scale-x-0 group-hover:scale-x-100 transition-transform duration-500 rounded-b-2xl" />
                </div>

                {/* Connecting arrow for desktop */}
                {index < 2 && (
                  <div className="hidden lg:block absolute top-1/2 -right-8 -translate-y-1/2 z-20 animate-fade-in delay-600">
                    <ArrowRight className="h-6 w-6 text-foreground/40 animate-pulse" />
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Additional visual interest - floating cards */}
          <div className="mt-16 flex flex-wrap items-center justify-center gap-6 text-sm animate-fade-up delay-700">
            {[
              { icon: CheckCircle, text: "No technical knowledge needed" },
              { icon: Sparkles, text: "Intuitive drag & drop interface" },
              { icon: Users, text: "Collaborate with your team" },
            ].map((item, index) => (
              <div
                key={index}
                className="flex items-center gap-2 bg-background/80 backdrop-blur-sm px-4 py-3 rounded-full border border-border hover:border-foreground/30 hover:shadow-lg hover:scale-105 transition-all duration-300"
              >
                <item.icon className="h-4 w-4 text-foreground" />
                <span className="text-muted-foreground font-medium">
                  {item.text}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
